using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AfetKoordinasyon.Veri;

namespace AfetKoordinasyon.Is
{
    // Haritada gösterilecek bir işaret (aynı konumdaki kayıtlar tek işarette toplanır)
    public class HaritaNoktasi
    {
        public double Enlem { get; set; }
        public double Boylam { get; set; }
        public string Baslik { get; set; }   // ör. "Merkez / Bingöl"
        public int Adet { get; set; }        // bu konumdaki kayıt sayısı
        public string Detay { get; set; }    // tıklayınca gösterilecek ayrıntı
    }

    public class HaritaServisi
    {
        // Tamamlanmamış ihtiyaçları KONUMA GÖRE gruplar (kümeleme).
        // Koordinatı henüz alınmamış konumlar haritada gösterilmez.
        public List<HaritaNoktasi> IhtiyacNoktalari()
        {
            using (var db = new AfetDbContext())
            {
                var ihtiyaclar = db.Ihtiyaclar
                    .Include(i => i.Konum)
                    .Include(i => i.Kategori)
                    .Include(i => i.Durum)
                    .Where(i => i.Durum.Ad != "Tamamlandı"
                             && i.Konum.Enlem != null && i.Konum.Boylam != null)
                    .ToList();

                return ihtiyaclar
                    .GroupBy(i => i.KonumId)
                    .Select(g =>
                    {
                        var konum = g.First().Konum;
                        return new HaritaNoktasi
                        {
                            Enlem = konum.Enlem.Value,
                            Boylam = konum.Boylam.Value,
                            Baslik = konum.Ilce + " / " + konum.Il,
                            Adet = g.Count(),
                            Detay = string.Join("\n", g.Select(i =>
                                "- " + i.Tur + " (" + i.Kategori.Ad + "), " + i.Miktar + " adet, " + i.Durum.Ad))
                        };
                    })
                    .ToList();
            }
        }

        // Gönüllüleri konuma göre gruplar
        public List<HaritaNoktasi> GonulluNoktalari()
        {
            using (var db = new AfetDbContext())
            {
                var gonulluler = db.Gonulluler
                    .Include(g => g.Konum)
                    .Where(g => g.Konum.Enlem != null && g.Konum.Boylam != null)
                    .ToList();

                return gonulluler
                    .GroupBy(g => g.KonumId)
                    .Select(grup =>
                    {
                        var konum = grup.First().Konum;
                        return new HaritaNoktasi
                        {
                            Enlem = konum.Enlem.Value,
                            Boylam = konum.Boylam.Value,
                            Baslik = konum.Ilce + " / " + konum.Il,
                            Adet = grup.Count(),
                            Detay = string.Join("\n", grup.Select(g =>
                                "- " + g.AdSoyad + " (" + g.Beceri + "), " + (g.MusaitMi ? "müsait" : "meşgul")))
                        };
                    })
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
