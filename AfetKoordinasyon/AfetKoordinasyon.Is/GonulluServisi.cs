using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Yeni gönüllü kaydı: sadece koordinatör ekleyebilir (Hafta 15: Gönüllü Yönetimi ekranına bağlandı)
        public void Ekle(Gonullu gonullu)
        {
            Oturum.RolGerekli("Koordinator");

            gonullu.AdSoyad = gonullu.AdSoyad?.Trim();
            gonullu.Telefon = gonullu.Telefon?.Replace(" ", "").Trim();
            gonullu.Beceri = string.IsNullOrWhiteSpace(gonullu.Beceri) ? null : gonullu.Beceri.Trim();

            if (string.IsNullOrWhiteSpace(gonullu.AdSoyad))
                throw new ArgumentException("Ad soyad boş olamaz.");
            if (gonullu.AdSoyad.Length > 100)
                throw new ArgumentException("Ad soyad en fazla 100 karakter olabilir.");
            if (gonullu.Telefon == null || !Regex.IsMatch(gonullu.Telefon, @"^05\d{9}$"))
                throw new ArgumentException("Telefon 05 ile başlayan 11 haneli olmalı (ör. 05XXXXXXXXX).");
            if (gonullu.Beceri != null && gonullu.Beceri.Length > 200)
                throw new ArgumentException("Beceri en fazla 200 karakter olabilir.");
            if (gonullu.KonumId <= 0)
                throw new ArgumentException("Bir konum seçin.");

            using (var db = new AfetDbContext())
            {
                if (db.Gonulluler.Any(g => g.Telefon == gonullu.Telefon))
                    throw new InvalidOperationException("Bu telefonla kayıtlı gönüllü var.");

                gonullu.MusaitMi = true;   // yeni gönüllü boşta başlar
                db.Gonulluler.Add(gonullu);
                db.SaveChanges();
            }
        }
    }
}
