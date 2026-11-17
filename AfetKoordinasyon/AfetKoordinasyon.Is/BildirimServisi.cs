using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AfetKoordinasyon.Veri;

namespace AfetKoordinasyon.Is
{
    public class KritikBildirim
    {
        public int IhtiyacId { get; set; }
        public string Ozet { get; set; }          // ör. "#14 Gıda (Genç), aciliyet 4/5"
        public double BeklemeSaati { get; set; }
    }

    // BİLDİRİM: aciliyeti yüksek olup uzun süredir gönüllü bekleyen ihtiyaçları bulur
    public class BildirimServisi
    {
        public const int VarsayilanEsikDakika = 60;   // 1 saatten uzun bekleyen kritik talep
        public const int EnAzAciliyet = 4;

        public List<KritikBildirim> UzunSuredirBekleyenKritikler(int esikDakika = VarsayilanEsikDakika)
        {
            using (var db = new AfetDbContext())
            {
                var kritikler = db.Ihtiyaclar
                    .Include(i => i.Konum)
                    .Include(i => i.DurumGecmisleri)
                    .Where(i => i.Durum.Ad == DurumAdlari.Bekliyor && i.AciliyetSkoru >= EnAzAciliyet)
                    .ToList();

                var simdi = DateTime.Now;
                return kritikler
                    .Select(i =>
                    {
                        // Bekleme, ihtiyacın EN SON "Bekliyor" durumuna geçtiği andan başlar
                        // (atama kaldırılıp tekrar beklemeye düşmüş olabilir); geçmiş yoksa bildirim tarihinden.
                        var baslangic = i.DurumGecmisleri
                            .Where(g => g.YeniDurum == DurumAdlari.Bekliyor && g.EskiDurum != DurumAdlari.Bekliyor)
                            .Select(g => (DateTime?)g.Tarih)
                            .Max() ?? i.Tarih;
                        return new KritikBildirim
                        {
                            IhtiyacId = i.IhtiyacId,
                            Ozet = "#" + i.IhtiyacId + " " + i.Tur + " (" + (i.Konum != null ? i.Konum.Ilce : "-") + "), aciliyet " + i.AciliyetSkoru + "/5",
                            BeklemeSaati = (simdi - baslangic).TotalHours
                        };
                    })
                    .Where(b => b.BeklemeSaati * 60 >= esikDakika)
                    .OrderByDescending(b => b.BeklemeSaati)
                    .ToList();
            }
        }

        // Önce dakikaya yuvarlanır, sonra birim seçilir.
        // (Eski hali 59 dk 57 sn için "60 dakikadır" yazıyordu; Hafta 13 birim testiyle bulundu.)
        public static string SureMetni(double saat)
        {
            int dakika = (int)Math.Round(saat * 60);
            if (dakika < 60) return dakika + " dakikadır";
            if (dakika < 48 * 60) return (dakika / 60) + " saattir";
            return (dakika / (24 * 60)) + " gündür";
        }
    }
}
