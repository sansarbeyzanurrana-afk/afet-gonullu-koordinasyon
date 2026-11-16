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

        // Bekleyen bir ihtiyacı gönüllüye atar, durumunu "Eşleşti" yapar.
        // Hafta 11: kurallar ve geçmiş kaydı DurumServisi'nde (tek yerde) tutuluyor.
        public void Eslestir(int ihtiyacId, int gonulluId)
        {
            new DurumServisi().Ata(ihtiyacId, gonulluId);
        }
        // Durumu "Bekliyor" olan ihtiyaçları listeler
        public List<Ihtiyac> BekleyenIhtiyaclariGetir()
        {
            using (var db = new AfetDbContext())
            {
                db.Configuration.LazyLoadingEnabled = false;

                return db.Ihtiyaclar
                         .Where(i => i.Durum.Ad == "Bekliyor")
                         .OrderBy(i => i.Tarih)
                         .ToList();
            }
        }
        // Durumu "Eşleşti" olan ihtiyaçları listeler
        public List<Ihtiyac> EslesmisIhtiyaclariGetir()
        {
            using (var db = new AfetDbContext())
            {
                db.Configuration.LazyLoadingEnabled = false;

                return db.Ihtiyaclar
                         .Where(i => i.Durum.Ad == "Eşleşti" || i.Durum.Ad == "Yolda")
                         .OrderBy(i => i.Tarih)
                         .ToList();
            }
        }
        // İhtiyaca uygun (müsait ve aynı konumdaki) gönüllüleri listeler
        public List<Gonullu> UygunGonulluleriGetir(int ihtiyacId)
        {
            using (var db = new AfetDbContext())
            {
                var ihtiyac = db.Ihtiyaclar.SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
                if (ihtiyac == null)
                    throw new InvalidOperationException("İhtiyaç bulunamadı.");

                return db.Gonulluler
                         .Where(g => g.MusaitMi && g.KonumId == ihtiyac.KonumId
                                  && !g.Eslesmeler.Any(e => e.Aktif))
                         .OrderBy(g => g.AdSoyad)
                         .ToList();
            }
        }
        // Eşleşmiş (ya da yoldaki) ihtiyacı "Tamamlandı" yapar, gönüllüyü tekrar müsait yapar
        public void Tamamla(int ihtiyacId)
        {
            new DurumServisi().Tamamla(ihtiyacId);
        }
    }
}