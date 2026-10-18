using System;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class KullaniciServisi
    {
        private const int MaksHataliGiris = 5;
        private const int KilitDakika = 15;

        public void KayitOl(string adSoyad, string email, string parola, string rol)
        {
            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Ad soyad ve e-posta zorunludur.");
            if (parola == null || parola.Length < 8)
                throw new ArgumentException("Parola en az 8 karakter olmalıdır.");
            if (rol != "Afetzede" && rol != "Gonullu")
                throw new ArgumentException("Geçersiz rol.");

            email = email.Trim().ToLowerInvariant();

            using (var db = new AfetDbContext())
            {
                if (db.Kullanicilar.Any(k => k.Email == email))
                    throw new InvalidOperationException("Bu e-posta zaten kayıtlı.");

                db.Kullanicilar.Add(new Kullanici
                {
                    AdSoyad = adSoyad.Trim(),
                    Email = email,
                    ParolaHash = BCrypt.Net.BCrypt.HashPassword(parola),
                    Rol = rol
                });
                db.SaveChanges();
            }
        }

        public Kullanici GirisYap(string email, string parola)
        {
            const string genelHata = "E-posta veya parola hatalı.";
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(parola))
                throw new UnauthorizedAccessException(genelHata);

            email = email.Trim().ToLowerInvariant();

            using (var db = new AfetDbContext())
            {
                var k = db.Kullanicilar.SingleOrDefault(x => x.Email == email);
                if (k == null)
                    throw new UnauthorizedAccessException(genelHata);

                if (k.KilitBitis.HasValue && k.KilitBitis > DateTime.Now)
                    throw new UnauthorizedAccessException(
                        "Çok fazla hatalı deneme. Hesap 15 dakika kilitlendi.");

                if (!BCrypt.Net.BCrypt.Verify(parola, k.ParolaHash))
                {
                    k.HataliGiris++;
                    if (k.HataliGiris >= MaksHataliGiris)
                    {
                        k.KilitBitis = DateTime.Now.AddMinutes(KilitDakika);
                        k.HataliGiris = 0;
                    }
                    db.SaveChanges();
                    throw new UnauthorizedAccessException(genelHata);
                }

                k.HataliGiris = 0;
                k.KilitBitis = null;
                db.SaveChanges();
                return k;
            }
        }
    }
}