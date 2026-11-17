using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    // Kategori / ilçe gibi bir gruba ait sayılar
    public class GrupSatiri
    {
        public string Ad { get; set; }
        public int Talep { get; set; }        // iptal hariç
        public int Karsilanan { get; set; }   // Tamamlandı
        public int Surecte { get; set; }      // Eşleşti + Yolda
        public int Bekleyen { get; set; }     // Bekliyor
        public int KritikBekleyen { get; set; }
        public string KarsilanmaOrani { get { return RaporServisi.OranMetni(Karsilanan, Talep); } }
    }

    public class GonulluSatiri
    {
        public string Gonullu { get; set; }
        public string Konum { get; set; }
        public string Durum { get; set; }         // Müsait / Görevde
        public int TamamlananIs { get; set; }
    }

    // Raporun tamamı
    public class DurumRaporu
    {
        public DateTime OlusturmaTarihi { get; set; }
        public string Donem { get; set; }

        public int Toplam { get; set; }
        public int Iptal { get; set; }
        public int Talep { get; set; }               // Toplam - İptal
        public int Karsilanan { get; set; }
        public int Surecte { get; set; }
        public int Bekleyen { get; set; }
        public int KritikBekleyen { get; set; }      // Bekliyor ve aciliyet 4-5

        public double? OrtAtanmaSaat { get; set; }   // bildirim -> ilk Eşleşti
        public int AtanmaOrnek { get; set; }
        public double? OrtKarsilanmaSaat { get; set; } // bildirim -> Tamamlandı
        public int KarsilanmaOrnek { get; set; }

        public int GonulluToplam { get; set; }
        public int GonulluMusait { get; set; }

        public List<GrupSatiri> Kategoriler { get; set; }
        public List<GrupSatiri> Ilceler { get; set; }
        public Dictionary<string, int> AciliyetDagilimi { get; set; }   // açık talepler için
        public List<GonulluSatiri> Gonulluler { get; set; }
        public List<string> VeriUyarilari { get; set; }

        public string KarsilanmaOrani { get { return RaporServisi.OranMetni(Karsilanan, Talep); } }
    }

    // HARİTA GÖRÜNÜMÜ VE DURUM RAPORU - hesaplar (toplulaştırma) burada yapılır
    public class RaporServisi
    {
        private static readonly CultureInfo tr = new CultureInfo("tr-TR");

        // gunSayisi: null = tüm zamanlar, 7 = son 7 gün ...
        public DurumRaporu Olustur(int? gunSayisi = null)
        {
            using (var db = new AfetDbContext())
            {
                var sorgu = db.Ihtiyaclar
                    .Include(i => i.Durum)
                    .Include(i => i.Kategori)
                    .Include(i => i.Konum)
                    .Include(i => i.DurumGecmisleri)
                    .Include(i => i.Eslesmeler.Select(e => e.Gonullu))
                    .AsQueryable();

                if (gunSayisi.HasValue)
                {
                    var baslangic = DateTime.Now.Date.AddDays(-gunSayisi.Value);
                    sorgu = sorgu.Where(i => i.Tarih >= baslangic);
                }
                var ihtiyaclar = sorgu.ToList();
                var gonulluler = db.Gonulluler.Include(g => g.Konum).ToList();

                var r = new DurumRaporu
                {
                    OlusturmaTarihi = DateTime.Now,
                    Donem = gunSayisi.HasValue ? "Son " + gunSayisi + " gün" : "Tüm zamanlar",
                    Toplam = ihtiyaclar.Count,
                    Iptal = ihtiyaclar.Count(i => i.Durum.Ad == DurumAdlari.Iptal)
                };

                // Geçerli talepler: iptal edilenler oranlara girmez
                var talepler = ihtiyaclar.Where(i => i.Durum.Ad != DurumAdlari.Iptal).ToList();
                r.Talep = talepler.Count;
                r.Karsilanan = talepler.Count(Karsilandi);
                r.Surecte = talepler.Count(Surecte);
                r.Bekleyen = talepler.Count(Bekliyor);
                r.KritikBekleyen = talepler.Count(Kritik);

                // --- Süreler (durum geçmişinden) ---
                var atanma = talepler
                    .Select(i => new { i.Tarih, Ilk = IlkGecis(i, DurumAdlari.Eslesti) })
                    .Where(x => x.Ilk.HasValue && x.Ilk.Value >= x.Tarih)
                    .Select(x => (x.Ilk.Value - x.Tarih).TotalHours)
                    .ToList();
                r.AtanmaOrnek = atanma.Count;
                r.OrtAtanmaSaat = atanma.Count > 0 ? atanma.Average() : (double?)null;

                var karsilanma = talepler.Where(Karsilandi)
                    .Select(i => new { i.Tarih, Son = IlkGecis(i, DurumAdlari.Tamamlandi) })
                    .Where(x => x.Son.HasValue && x.Son.Value >= x.Tarih)
                    .Select(x => (x.Son.Value - x.Tarih).TotalHours)
                    .ToList();
                r.KarsilanmaOrnek = karsilanma.Count;
                r.OrtKarsilanmaSaat = karsilanma.Count > 0 ? karsilanma.Average() : (double?)null;

                // --- Gruplara göre toplulaştırma (GROUP BY) ---
                r.Kategoriler = Grupla(talepler, i => i.Kategori != null ? i.Kategori.Ad : "(kategorisiz)");
                r.Ilceler = Grupla(talepler, i => i.Konum != null ? i.Konum.Ilce + " / " + i.Konum.Il : "(konumsuz)");

                // --- Açık taleplerin aciliyet dağılımı ---
                var acik = talepler.Where(i => !Karsilandi(i)).ToList();
                r.AciliyetDagilimi = new Dictionary<string, int>();
                for (int s = 5; s >= 1; s--)
                {
                    int skor = s;
                    r.AciliyetDagilimi[skor + "/5"] = acik.Count(i => i.AciliyetSkoru == skor);
                }
                r.AciliyetDagilimi["Değerlendirilmemiş"] = acik.Count(i => !i.AciliyetSkoru.HasValue);

                // --- Gönüllüler ---
                // Tamamlanan iş: tamamlanan ihtiyacın EN SON eşleşmesindeki gönüllüye yazılır
                var tamamlayanlar = talepler.Where(Karsilandi)
                    .Select(i => i.Eslesmeler.OrderByDescending(e => e.Tarih).FirstOrDefault())
                    .Where(e => e != null)
                    .GroupBy(e => e.GonulluId)
                    .ToDictionary(g => g.Key, g => g.Count());

                r.GonulluToplam = gonulluler.Count;
                r.GonulluMusait = gonulluler.Count(g => g.MusaitMi);
                r.Gonulluler = gonulluler
                    .Select(g => new GonulluSatiri
                    {
                        Gonullu = g.AdSoyad,
                        Konum = g.Konum != null ? g.Konum.Ilce : "-",
                        Durum = g.MusaitMi ? "Müsait" : "Görevde",
                        TamamlananIs = tamamlayanlar.ContainsKey(g.GonulluId) ? tamamlayanlar[g.GonulluId] : 0
                    })
                    .OrderByDescending(g => g.TamamlananIs).ThenBy(g => g.Gonullu)
                    .ToList();

                // --- Veri kalitesi: eksik veri sonucu nasıl etkiler? ---
                r.VeriUyarilari = new List<string>();
                int skorsuz = acik.Count(i => !i.AciliyetSkoru.HasValue);
                if (skorsuz > 0)
                    r.VeriUyarilari.Add(skorsuz + " açık talebin aciliyet skoru yok; \"kritik bekleyen\" sayısı olduğundan düşük görünebilir.");
                int gecmissizKarsilanan = talepler.Count(i => Karsilandi(i) && !IlkGecis(i, DurumAdlari.Tamamlandi).HasValue);
                if (gecmissizKarsilanan > 0)
                    r.VeriUyarilari.Add(gecmissizKarsilanan + " tamamlanan talep, durum geçmişi tutulmaya başlamadan önce kapatılmış; ortalama süre hesabına katılmadı (hesaplanan: " + r.KarsilanmaOrnek + " kayıt).");
                int koordinatsiz = talepler.Count(i => i.Konum != null && (!i.Konum.Enlem.HasValue || !i.Konum.Boylam.HasValue));
                if (koordinatsiz > 0)
                    r.VeriUyarilari.Add(koordinatsiz + " talebin konumunun koordinatı yok; bu talepler haritada görünmez.");
                if (r.Talep < 10)
                    r.VeriUyarilari.Add("Talep sayısı az (" + r.Talep + "); oranlar küçük değişikliklerle çok oynar, sayılarla birlikte okunmalı.");
                if (r.VeriUyarilari.Count == 0)
                    r.VeriUyarilari.Add("Belirgin bir eksik veri bulunmadı.");

                return r;
            }
        }

        // Rapor özet metni (insan dilinde): kurallarla oluşturulur
        public string OzetMetni(DurumRaporu r)
        {
            var sb = new StringBuilder();
            sb.Append(r.Donem + " içinde " + r.Toplam + " ihtiyaç bildirildi");
            if (r.Iptal > 0) sb.Append(" (" + r.Iptal + " tanesi iptal edildi)");
            sb.Append(". Geçerli " + r.Talep + " talebin " + r.Karsilanan + " tanesi karşılandı (" + r.KarsilanmaOrani + "), ");
            sb.Append(r.Surecte + " tanesi süreçte, " + r.Bekleyen + " tanesi bekliyor.");
            if (r.KritikBekleyen > 0)
                sb.Append(" Dikkat: aciliyeti 4-5 olan " + r.KritikBekleyen + " talep hâlâ gönüllü bekliyor.");
            var enCokBekleyen = r.Ilceler.OrderByDescending(g => g.Bekleyen).FirstOrDefault();
            if (enCokBekleyen != null && enCokBekleyen.Bekleyen > 0)
                sb.Append(" En çok bekleyen talep " + enCokBekleyen.Ad + " bölgesinde (" + enCokBekleyen.Bekleyen + ").");
            sb.Append(" Müsait gönüllü: " + r.GonulluMusait + "/" + r.GonulluToplam + ".");
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // DIŞA AKTARMA (CSV): Türkçe Excel ayırıcı olarak ";" bekler
        // ------------------------------------------------------------------
        public string CsvOlustur(DurumRaporu r)
        {
            var sb = new StringBuilder();
            Satir(sb, "AFET SONRASI İHTİYAÇ VE GÖNÜLLÜ KOORDİNASYON - DURUM RAPORU");
            Satir(sb, "Oluşturma tarihi", r.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm", tr));
            Satir(sb, "Dönem", r.Donem);
            sb.AppendLine();

            Satir(sb, "ÖZET");
            Satir(sb, "Metrik", "Değer", "Açıklama");
            Satir(sb, "Toplam bildirim", r.Toplam.ToString(), "iptal dahil");
            Satir(sb, "İptal", r.Iptal.ToString(), "");
            Satir(sb, "Geçerli talep", r.Talep.ToString(), "toplam - iptal");
            Satir(sb, "Karşılanan", r.Karsilanan.ToString(), "durum: Tamamlandı");
            Satir(sb, "Süreçte", r.Surecte.ToString(), "durum: Eşleşti veya Yolda");
            Satir(sb, "Bekleyen", r.Bekleyen.ToString(), "durum: Bekliyor");
            Satir(sb, "Kritik bekleyen", r.KritikBekleyen.ToString(), "Bekliyor ve aciliyet 4-5");
            Satir(sb, "Karşılanma oranı", r.KarsilanmaOrani, "karşılanan / geçerli talep");
            Satir(sb, "Ortalama atanma süresi", SureMetni(r.OrtAtanmaSaat, r.AtanmaOrnek), "bildirim -> ilk eşleşme (durum geçmişinden)");
            Satir(sb, "Ortalama karşılanma süresi", SureMetni(r.OrtKarsilanmaSaat, r.KarsilanmaOrnek), "bildirim -> tamamlandı (durum geçmişinden)");
            // "1/2" yazılırsa Excel tarih sanıp "1.Şub" yapıyor; bu yüzden metin olarak yazılır
            Satir(sb, "Müsait gönüllü", r.GonulluMusait + " müsait / " + r.GonulluToplam + " toplam", "");
            Satir(sb, "Özet metni", OzetMetni(r), "");
            sb.AppendLine();

            GrupTablosu(sb, "KATEGORİYE GÖRE", "Kategori", r.Kategoriler);
            GrupTablosu(sb, "BÖLGEYE (İLÇE) GÖRE", "Bölge", r.Ilceler);

            Satir(sb, "AÇIK TALEPLERİN ACİLİYET DAĞILIMI");
            Satir(sb, "Aciliyet", "Talep sayısı");
            foreach (var kv in r.AciliyetDagilimi) Satir(sb, kv.Key, kv.Value.ToString());
            sb.AppendLine();

            Satir(sb, "GÖNÜLLÜLER");
            Satir(sb, "Gönüllü", "Bölge", "Durum", "Tamamlanan iş");
            foreach (var g in r.Gonulluler) Satir(sb, g.Gonullu, g.Konum, g.Durum, g.TamamlananIs.ToString());
            sb.AppendLine();

            Satir(sb, "VERİ KALİTESİ UYARILARI");
            foreach (var u in r.VeriUyarilari) Satir(sb, u);
            return sb.ToString();
        }

        private static void GrupTablosu(StringBuilder sb, string baslik, string adBasligi, List<GrupSatiri> gruplar)
        {
            Satir(sb, baslik);
            Satir(sb, adBasligi, "Talep", "Karşılanan", "Süreçte", "Bekleyen", "Kritik bekleyen", "Karşılanma oranı");
            foreach (var g in gruplar)
                Satir(sb, g.Ad, g.Talep.ToString(), g.Karsilanan.ToString(), g.Surecte.ToString(),
                      g.Bekleyen.ToString(), g.KritikBekleyen.ToString(), g.KarsilanmaOrani);
            Satir(sb, "TOPLAM", gruplar.Sum(g => g.Talep).ToString(), gruplar.Sum(g => g.Karsilanan).ToString(),
                  gruplar.Sum(g => g.Surecte).ToString(), gruplar.Sum(g => g.Bekleyen).ToString(),
                  gruplar.Sum(g => g.KritikBekleyen).ToString(),
                  OranMetni(gruplar.Sum(g => g.Karsilanan), gruplar.Sum(g => g.Talep)));
            sb.AppendLine();
        }

        // Bir CSV satırı: alan ";" , tırnak ya da satır sonu içeriyorsa tırnak içine alınır
        private static void Satir(StringBuilder sb, params string[] alanlar)
        {
            sb.AppendLine(string.Join(";", alanlar.Select(a =>
            {
                a = a ?? "";
                return a.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0
                    ? "\"" + a.Replace("\"", "\"\"") + "\""
                    : a;
            })));
        }

        // ------------------------------------------------------------------
        // Yardımcılar
        // ------------------------------------------------------------------
        private static bool Karsilandi(Ihtiyac i) { return i.Durum.Ad == DurumAdlari.Tamamlandi; }
        private static bool Bekliyor(Ihtiyac i) { return i.Durum.Ad == DurumAdlari.Bekliyor; }
        private static bool Surecte(Ihtiyac i) { return i.Durum.Ad == DurumAdlari.Eslesti || i.Durum.Ad == DurumAdlari.Yolda; }
        private static bool Kritik(Ihtiyac i) { return Bekliyor(i) && i.AciliyetSkoru >= 4; }

        // Durum geçmişinde bir duruma İLK geçiş zamanı (yoksa null)
        private static DateTime? IlkGecis(Ihtiyac i, string durum)
        {
            return i.DurumGecmisleri
                    .Where(g => g.YeniDurum == durum && g.EskiDurum != null && g.EskiDurum != durum)
                    .Select(g => (DateTime?)g.Tarih)
                    .Min();
        }

        private static List<GrupSatiri> Grupla(List<Ihtiyac> talepler, Func<Ihtiyac, string> anahtar)
        {
            return talepler
                .GroupBy(anahtar)
                .Select(g => new GrupSatiri
                {
                    Ad = g.Key,
                    Talep = g.Count(),
                    Karsilanan = g.Count(Karsilandi),
                    Surecte = g.Count(Surecte),
                    Bekleyen = g.Count(Bekliyor),
                    KritikBekleyen = g.Count(Kritik)
                })
                .OrderByDescending(g => g.Bekleyen).ThenByDescending(g => g.Talep).ThenBy(g => g.Ad)
                .ToList();
        }

        // Oran her zaman sayıyla birlikte: "%33 (6/18)". Payda 0 ise "-".
        public static string OranMetni(int pay, int payda)
        {
            if (payda == 0) return "-";
            return "%" + Math.Round(100.0 * pay / payda).ToString("0", tr) + " (" + pay + "/" + payda + ")";
        }

        public static string SureMetni(double? saat, int ornek)
        {
            if (!saat.HasValue) return "veri yok";
            var s = saat.Value < 24
                ? saat.Value.ToString("0.0", tr) + " saat"
                : (saat.Value / 24).ToString("0.0", tr) + " gün";
            return s + " (" + ornek + " kayıt)";
        }
    }
}
