using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    // Durum adları tek yerde: kodun içinde "Eşleşti" gibi metinler dağınık yazılmasın
    public static class DurumAdlari
    {
        public const string Bekliyor = "Bekliyor";
        public const string Eslesti = "Eşleşti";
        public const string Yolda = "Yolda";
        public const string Tamamlandi = "Tamamlandı";
        public const string Iptal = "İptal";
    }

    // Durum takibi ekranındaki bir satır
    public class TakipSatiri
    {
        public int IhtiyacId { get; set; }
        public string Tur { get; set; }
        public string Aciklama { get; set; }
        public string Kategori { get; set; }
        public string Konum { get; set; }
        public string Durum { get; set; }
        public int? Aciliyet { get; set; }
        public string Gonullu { get; set; }          // aktif eşleşmedeki gönüllü (varsa)
        public DateTime? SonDegisiklik { get; set; }
        public byte[] SatirSurumu { get; set; }      // eşzamanlılık kontrolü için
    }

    // Geçmiş listesindeki bir satır
    public class GecmisSatiri
    {
        public DateTime Tarih { get; set; }
        public string Degisiklik { get; set; }       // "Eşleşti → Yolda" ya da "Not"
        public string Kullanici { get; set; }
        public string Not { get; set; }
    }

    // DURUM TAKİBİ VE KOORDİNASYON MODÜLÜ
    // Bir ihtiyacın durumunu SADECE bu servis değiştirir; her değişiklik geçmişe yazılır.
    public class DurumServisi
    {
        // ------------------------------------------------------------------
        // DURUM MAKİNESİ: hangi durumdan hangi duruma geçilebilir?
        // ------------------------------------------------------------------
        private static readonly Dictionary<string, string[]> IzinliGecisler = new Dictionary<string, string[]>
        {
            { DurumAdlari.Bekliyor,   new[] { DurumAdlari.Eslesti, DurumAdlari.Iptal } },
            { DurumAdlari.Eslesti,    new[] { DurumAdlari.Yolda, DurumAdlari.Tamamlandi, DurumAdlari.Bekliyor, DurumAdlari.Iptal } },
            { DurumAdlari.Yolda,      new[] { DurumAdlari.Tamamlandi, DurumAdlari.Bekliyor, DurumAdlari.Iptal } },
            { DurumAdlari.Tamamlandi, new string[0] },   // son durum
            { DurumAdlari.Iptal,      new string[0] },   // son durum
        };

        public static string[] SonrakiDurumlar(string mevcutDurum)
        {
            string[] sonraki;
            return IzinliGecisler.TryGetValue(mevcutDurum ?? "", out sonraki) ? sonraki : new string[0];
        }

        public static bool GecisGecerliMi(string eski, string yeni)
        {
            return SonrakiDurumlar(eski).Contains(yeni);
        }

        // ------------------------------------------------------------------
        // ATAMA / GÜNCELLEME İŞLEMLERİ
        // ------------------------------------------------------------------

        // Bekliyor -> Eşleşti
        public void Ata(int ihtiyacId, int gonulluId, byte[] satirSurumu = null, string not = null)
        {
            Oturum.RolGerekli("Koordinator");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, satirSurumu);
                if (ihtiyac.Durum.Ad != DurumAdlari.Bekliyor)
                    throw new InvalidOperationException(
                        "Sadece 'Bekliyor' durumundaki ihtiyaçlara gönüllü atanabilir. Bu ihtiyacın durumu: " + ihtiyac.Durum.Ad);

                var gonullu = db.Gonulluler.SingleOrDefault(g => g.GonulluId == gonulluId);
                if (gonullu == null)
                    throw new InvalidOperationException("Gönüllü bulunamadı.");

                // ÇİFT ATAMA KONTROLÜ (kod tarafı): gönüllünün başka aktif işi olmamalı
                if (!gonullu.MusaitMi || db.Eslesmeler.Any(e => e.GonulluId == gonulluId && e.Aktif))
                    throw new InvalidOperationException("Bu gönüllü şu an başka bir işte, müsait değil.");

                db.Eslesmeler.Add(new Eslesme
                {
                    IhtiyacId = ihtiyacId,
                    GonulluId = gonulluId,
                    Tarih = DateTime.Now,
                    Aktif = true
                });
                gonullu.MusaitMi = false;

                DurumDegistir(db, ihtiyac, DurumAdlari.Eslesti,
                    string.IsNullOrWhiteSpace(not) ? "Gönüllü atandı: " + gonullu.AdSoyad : not);
                Kaydet(db);
            }
        }

        // Eşleşti -> Yolda
        public void YolaCikti(int ihtiyacId, byte[] satirSurumu = null, string not = null)
        {
            Oturum.RolGerekli("Koordinator");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, satirSurumu);
                DurumDegistir(db, ihtiyac, DurumAdlari.Yolda, not);
                Kaydet(db);
            }
        }

        // Eşleşti / Yolda -> Tamamlandı: gönüllü tekrar müsait olur
        public void Tamamla(int ihtiyacId, byte[] satirSurumu = null, string not = null)
        {
            Oturum.RolGerekli("Koordinator");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, satirSurumu);
                DurumDegistir(db, ihtiyac, DurumAdlari.Tamamlandi, not);
                AktifEslesmeyiKapat(db, ihtiyacId);
                Kaydet(db);
            }
        }

        // Eşleşti / Yolda -> Bekliyor (gönüllü gidemedi, yeniden atanacak). NOT ZORUNLU.
        public void AtamayiKaldir(int ihtiyacId, string not, byte[] satirSurumu = null)
        {
            Oturum.RolGerekli("Koordinator");
            NotZorunlu(not, "Atamayı kaldırma nedeni");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, satirSurumu);
                DurumDegistir(db, ihtiyac, DurumAdlari.Bekliyor, not);
                AktifEslesmeyiKapat(db, ihtiyacId);
                Kaydet(db);
            }
        }

        // Herhangi bir aktif durum -> İptal. NOT ZORUNLU.
        public void IptalEt(int ihtiyacId, string not, byte[] satirSurumu = null)
        {
            Oturum.RolGerekli("Koordinator");
            NotZorunlu(not, "İptal nedeni");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, satirSurumu);
                DurumDegistir(db, ihtiyac, DurumAdlari.Iptal, not);
                AktifEslesmeyiKapat(db, ihtiyacId);
                Kaydet(db);
            }
        }

        // Durum değiştirmeden not ekler. SAHİPLİK KONTROLÜ: koordinatör ya da ihtiyacı bildiren kişi.
        public void NotEkle(int ihtiyacId, string not)
        {
            if (!Oturum.Aktif)
                throw new UnauthorizedAccessException("Oturum süresi doldu, lütfen tekrar giriş yapın.");
            NotZorunlu(not, "Not");
            using (var db = new AfetDbContext())
            {
                var ihtiyac = IhtiyaciGetir(db, ihtiyacId, null);
                var k = Oturum.AktifKullanici;
                bool koordinator = string.Equals(k.Rol?.Trim(), "Koordinator", StringComparison.OrdinalIgnoreCase);
                if (!koordinator && ihtiyac.KullaniciId != k.Id)
                    throw new UnauthorizedAccessException("Sadece koordinatör ya da ihtiyacı bildiren kişi not ekleyebilir.");

                db.DurumGecmisleri.Add(new DurumGecmisi
                {
                    IhtiyacId = ihtiyacId,
                    EskiDurum = ihtiyac.Durum.Ad,
                    YeniDurum = ihtiyac.Durum.Ad,
                    Tarih = DateTime.Now,
                    KullaniciId = k.Id,
                    Not = not.Trim()
                });
                Kaydet(db);
            }
        }

        // ------------------------------------------------------------------
        // LİSTELEME
        // ------------------------------------------------------------------
        public List<TakipSatiri> TakipListesi(string durum = null, int? enAzAciliyet = null)
        {
            using (var db = new AfetDbContext())
            {
                var sorgu = db.Ihtiyaclar.AsQueryable();
                if (!string.IsNullOrEmpty(durum))
                    sorgu = sorgu.Where(i => i.Durum.Ad == durum);
                if (enAzAciliyet.HasValue)
                    sorgu = sorgu.Where(i => i.AciliyetSkoru >= enAzAciliyet.Value);

                return sorgu
                    .OrderByDescending(i => i.AciliyetSkoru ?? 0)
                    .ThenByDescending(i => i.Tarih)
                    .Select(i => new TakipSatiri
                    {
                        IhtiyacId = i.IhtiyacId,
                        Tur = i.Tur,
                        Aciklama = i.Aciklama,
                        Kategori = i.Kategori.Ad,
                        Konum = i.Konum.Il + " / " + i.Konum.Ilce,
                        Durum = i.Durum.Ad,
                        Aciliyet = i.AciliyetSkoru,
                        Gonullu = i.Eslesmeler.Where(e => e.Aktif).Select(e => e.Gonullu.AdSoyad).FirstOrDefault(),
                        SonDegisiklik = i.DurumGecmisleri.Max(g => (DateTime?)g.Tarih),
                        SatirSurumu = i.SatirSurumu
                    })
                    .ToList();
            }
        }

        public List<GecmisSatiri> GecmisiGetir(int ihtiyacId)
        {
            using (var db = new AfetDbContext())
            {
                return db.DurumGecmisleri
                    .Where(g => g.IhtiyacId == ihtiyacId)
                    .OrderByDescending(g => g.Tarih)
                    .ThenByDescending(g => g.DurumGecmisiId)
                    .Select(g => new
                    {
                        g.Tarih,
                        g.EskiDurum,
                        g.YeniDurum,
                        Kullanici = g.Kullanici != null ? g.Kullanici.AdSoyad : "-",
                        g.Not
                    })
                    .ToList()
                    .Select(g => new GecmisSatiri
                    {
                        Tarih = g.Tarih,
                        Degisiklik = g.EskiDurum == null ? "Başlangıç: " + g.YeniDurum
                                   : g.EskiDurum == g.YeniDurum ? "Not"
                                   : g.EskiDurum + " → " + g.YeniDurum,
                        Kullanici = g.Kullanici,
                        Not = g.Not
                    })
                    .ToList();
            }
        }

        // ------------------------------------------------------------------
        // YARDIMCILAR
        // ------------------------------------------------------------------

        // Tüm durum değişiklikleri buradan geçer: kural kontrolü + geçmiş kaydı
        internal static void DurumDegistir(AfetDbContext db, Ihtiyac ihtiyac, string yeniDurum, string not)
        {
            var eskiDurum = ihtiyac.Durum.Ad;
            if (!GecisGecerliMi(eskiDurum, yeniDurum))
                throw new InvalidOperationException(
                    "'" + eskiDurum + "' durumundaki bir ihtiyaç '" + yeniDurum + "' durumuna geçirilemez.");

            ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == yeniDurum).DurumId;

            db.DurumGecmisleri.Add(new DurumGecmisi
            {
                IhtiyacId = ihtiyac.IhtiyacId,
                EskiDurum = eskiDurum,
                YeniDurum = yeniDurum,
                Tarih = DateTime.Now,
                KullaniciId = Oturum.AktifKullanici?.Id,
                Not = string.IsNullOrWhiteSpace(not) ? null : not.Trim()
            });
        }

        private static Ihtiyac IhtiyaciGetir(AfetDbContext db, int ihtiyacId, byte[] satirSurumu)
        {
            var ihtiyac = db.Ihtiyaclar.Include(i => i.Durum).SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
            if (ihtiyac == null)
                throw new InvalidOperationException("İhtiyaç bulunamadı.");

            // Kullanıcının ekranda gördüğü sürümü "beklenen sürüm" olarak ver:
            // arada başka biri değiştirdiyse SaveChanges eşzamanlılık hatası verir.
            if (satirSurumu != null)
                db.Entry(ihtiyac).Property(i => i.SatirSurumu).OriginalValue = satirSurumu;
            return ihtiyac;
        }

        // Aktif eşleşme silinmez, pasif yapılır; gönüllü tekrar müsait olur
        private static void AktifEslesmeyiKapat(AfetDbContext db, int ihtiyacId)
        {
            var eslesme = db.Eslesmeler.Include(e => e.Gonullu)
                            .FirstOrDefault(e => e.IhtiyacId == ihtiyacId && e.Aktif);
            if (eslesme == null) return;
            eslesme.Aktif = false;
            eslesme.BitisTarihi = DateTime.Now;
            eslesme.Gonullu.MusaitMi = true;
        }

        private static void NotZorunlu(string not, string alan)
        {
            if (string.IsNullOrWhiteSpace(not) || not.Trim().Length < 5)
                throw new ArgumentException(alan + " en az 5 karakter olmalıdır.");
            if (not.Trim().Length > 500)
                throw new ArgumentException(alan + " en fazla 500 karakter olabilir.");
        }

        // Tek SaveChanges = tek işlem (transaction): ya hepsi kaydedilir ya hiçbiri
        private static void Kaydet(AfetDbContext db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    "Bu ihtiyaç siz işlem yaparken başka biri tarafından değiştirildi. Liste yenilendi, lütfen tekrar deneyin.");
            }
            catch (DbUpdateException ex) when (BenzersizlikIhlali(ex))
            {
                throw new InvalidOperationException(
                    "Çift atama engellendi: bu ihtiyacın ya da gönüllünün zaten aktif bir eşleşmesi var.");
            }
        }

        private static bool BenzersizlikIhlali(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                var sql = e as SqlException;
                if (sql != null && (sql.Number == 2601 || sql.Number == 2627))
                    return true;
            }
            return false;
        }
    }
}
