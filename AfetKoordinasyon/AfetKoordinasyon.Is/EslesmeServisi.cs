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
            Oturum.RolGerekli("Koordinator");

            using (var db = new AfetDbContext())
            {
                var ihtiyac = db.Ihtiyaclar.Include(i => i.Durum)
                                .SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
                if (ihtiyac == null)
                    throw new InvalidOperationException("İhtiyaç bulunamadı.");
                if (ihtiyac.Durum.Ad != "Bekliyor")
                    throw new InvalidOperationException("Bu ihtiyaç zaten eşleştirilmiş.");

                var gonullu = db.Gonulluler.SingleOrDefault(g => g.GonulluId == gonulluId);
                if (gonullu == null)
                    throw new InvalidOperationException("Gönüllü bulunamadı.");
                if (!gonullu.MusaitMi)
                    throw new InvalidOperationException("Bu gönüllü şu an müsait değil.");

                db.Eslesmeler.Add(new Eslesme
                {
                    IhtiyacId = ihtiyacId,
                    GonulluId = gonulluId,
                    Tarih = DateTime.Now
                });

                ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == "Eşleşti").DurumId;
                gonullu.MusaitMi = false;
                db.SaveChanges();
            }
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
                         .Where(i => i.Durum.Ad == "Eşleşti")
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
                         .Where(g => g.MusaitMi && g.KonumId == ihtiyac.KonumId)
                         .OrderBy(g => g.AdSoyad)
                         .ToList();
            }
        }
        // Eşleşmiş ihtiyacı "Tamamlandı" yapar
        public void Tamamla(int ihtiyacId)
        {
            Oturum.RolGerekli("Koordinator");

            using (var db = new AfetDbContext())
            {
                var ihtiyac = db.Ihtiyaclar.Include(i => i.Durum)
                                .SingleOrDefault(i => i.IhtiyacId == ihtiyacId);
                if (ihtiyac == null)
                    throw new InvalidOperationException("İhtiyaç bulunamadı.");
                if (ihtiyac.Durum.Ad != "Eşleşti")
                    throw new InvalidOperationException("Sadece eşleşmiş ihtiyaçlar tamamlanabilir.");

                ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == "Tamamlandı").DurumId;
                var eslesme = db.Eslesmeler.Include(e => e.Gonullu)
                           .FirstOrDefault(e => e.IhtiyacId == ihtiyacId);
                if (eslesme != null)
                    eslesme.Gonullu.MusaitMi = true;
                db.SaveChanges();
            }
        }
    }
}