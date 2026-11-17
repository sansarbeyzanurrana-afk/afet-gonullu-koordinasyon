using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AfetKoordinasyon.Veri;

namespace AfetKoordinasyon.Is
{
    // Bir bölgenin (konumun) harita özeti: o konumdaki tüm kayıtlar tek satırda toplanır (kümeleme)
    public class BolgeOzeti
    {
        public int KonumId { get; set; }
        public string Baslik { get; set; }          // ör. "Genç / Bingöl"
        public double? Enlem { get; set; }
        public double? Boylam { get; set; }
        public bool KoordinatVar { get { return Enlem.HasValue && Boylam.HasValue; } }

        public int Bekleyen { get; set; }
        public int KritikBekleyen { get; set; }      // Bekliyor ve aciliyet 4-5
        public int Surecte { get; set; }             // Eşleşti + Yolda
        public int Karsilanan { get; set; }          // Tamamlandı
        public int GonulluToplam { get; set; }
        public int GonulluMusait { get; set; }

        public string AcikTalepDetayi { get; set; }  // tıklayınca gösterilecek liste
        public string GonulluDetayi { get; set; }
    }

    public class HaritaServisi
    {
        // Her konum için durum sayıları. İptal edilen talepler sayılmaz.
        public List<BolgeOzeti> BolgeOzetleri()
        {
            using (var db = new AfetDbContext())
            {
                var konumlar = db.Konumlar.ToList();
                var ihtiyaclar = db.Ihtiyaclar
                    .Include(i => i.Durum)
                    .Include(i => i.Kategori)
                    .Where(i => i.Durum.Ad != DurumAdlari.Iptal)
                    .ToList();
                var gonulluler = db.Gonulluler.ToList();

                return konumlar.Select(k =>
                {
                    var buradaki = ihtiyaclar.Where(i => i.KonumId == k.KonumId).ToList();
                    var acik = buradaki
                        .Where(i => i.Durum.Ad != DurumAdlari.Tamamlandi)
                        .OrderByDescending(i => i.AciliyetSkoru ?? 0)
                        .ToList();
                    var gonullu = gonulluler.Where(g => g.KonumId == k.KonumId).ToList();

                    return new BolgeOzeti
                    {
                        KonumId = k.KonumId,
                        Baslik = k.Ilce + " / " + k.Il,
                        Enlem = k.Enlem,
                        Boylam = k.Boylam,
                        Bekleyen = buradaki.Count(i => i.Durum.Ad == DurumAdlari.Bekliyor),
                        KritikBekleyen = buradaki.Count(i => i.Durum.Ad == DurumAdlari.Bekliyor && i.AciliyetSkoru >= 4),
                        Surecte = buradaki.Count(i => i.Durum.Ad == DurumAdlari.Eslesti || i.Durum.Ad == DurumAdlari.Yolda),
                        Karsilanan = buradaki.Count(i => i.Durum.Ad == DurumAdlari.Tamamlandi),
                        GonulluToplam = gonullu.Count,
                        GonulluMusait = gonullu.Count(g => g.MusaitMi),
                        AcikTalepDetayi = string.Join("\n", acik.Select(i =>
                            "- #" + i.IhtiyacId + " " + i.Tur + " (" + (i.Kategori != null ? i.Kategori.Ad : "-") + "), "
                            + i.Durum.Ad + ", aciliyet " + (i.AciliyetSkoru.HasValue ? i.AciliyetSkoru + "/5" : "-"))),
                        GonulluDetayi = string.Join("\n", gonullu.Select(g =>
                            "- " + g.AdSoyad + " (" + g.Beceri + "), " + (g.MusaitMi ? "müsait" : "görevde")))
                    };
                })
                // Hiç kaydı olmayan konumları gösterme
                .Where(b => b.Bekleyen + b.Surecte + b.Karsilanan + b.GonulluToplam > 0)
                .OrderByDescending(b => b.KritikBekleyen).ThenByDescending(b => b.Bekleyen).ThenBy(b => b.Baslik)
                .ToList();
            }
        }

        // Koordinatı henüz alınmamış konum sayısı
        public int KoordinatiEksikKonumSayisi()
        {
            using (var db = new AfetDbContext())
            {
                return db.Konumlar.Count(k => k.Enlem == null || k.Boylam == null);
            }
        }
    }
}
