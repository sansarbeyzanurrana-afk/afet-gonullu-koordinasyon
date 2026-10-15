using System;
using System.Collections.Generic;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class GonulluServisi
    {
        public List<Gonullu> Listele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Gonulluler.OrderBy(g => g.AdSoyad).ToList();
            }
        }

        public void Ekle(Gonullu gonullu)
        {
            if (string.IsNullOrWhiteSpace(gonullu.AdSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.");
            if (string.IsNullOrWhiteSpace(gonullu.Telefon) || gonullu.Telefon.Length != 11)
                throw new ArgumentException("Telefon 11 haneli olmalı (05xx...).");

            using (var db = new AfetDbContext())
            {
                if (db.Gonulluler.Any(g => g.Telefon == gonullu.Telefon))
                    throw new InvalidOperationException("Bu telefonla kayıtlı gönüllü var.");

                db.Gonulluler.Add(gonullu);
                db.SaveChanges();
            }
        }
    }
}