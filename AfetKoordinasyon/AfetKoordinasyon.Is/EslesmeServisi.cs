using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class EslesmeServisi
    {
        public List<Eslesme> Listele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Eslesmeler
                         .Include(e => e.Ihtiyac)
                         .Include(e => e.Gonullu)
                         .OrderByDescending(e => e.Tarih)
                         .ToList();
            }
        }

        // Bekleyen bir ihtiyacı gönüllüye atar, durumunu "Eşleşti" yapar
        public void Eslestir(int ihtiyacId, int gonulluId)
        {
            using (var db = new AfetDbContext())
            {
                var ihtiyac = db.Ihtiyaclar.Include(i => i.Durum)
                                .SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
                if (ihtiyac == null)
                    throw new InvalidOperationException("İhtiyaç bulunamadı.");
                if (ihtiyac.Durum.Ad != "Bekliyor")
                    throw new InvalidOperationException("Bu ihtiyaç zaten eşleştirilmiş.");

                if (!db.Gonulluler.Any(g => g.GonulluId == gonulluId))
                    throw new InvalidOperationException("Gönüllü bulunamadı.");

                db.Eslesmeler.Add(new Eslesme
                {
                    IhtiyacId = ihtiyacId,
                    GonulluId = gonulluId,
                    Tarih = DateTime.Now
                });

                ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == "Eşleşti").DurumId;
                db.SaveChanges();
            }
        }

        // Eşleşmiş ihtiyacı "Tamamlandı" yapar
        public void Tamamla(int ihtiyacId)
        {
            using (var db = new AfetDbContext())
            {
                var ihtiyac = db.Ihtiyaclar.Include(i => i.Durum)
                                .SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
                if (ihtiyac == null)
                    throw new InvalidOperationException("İhtiyaç bulunamadı.");
                if (ihtiyac.Durum.Ad != "Eşleşti")
                    throw new InvalidOperationException("Sadece eşleşmiş ihtiyaçlar tamamlanabilir.");

                ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == "Tamamlandı").DurumId;
                db.SaveChanges();
            }
        }
    }
}