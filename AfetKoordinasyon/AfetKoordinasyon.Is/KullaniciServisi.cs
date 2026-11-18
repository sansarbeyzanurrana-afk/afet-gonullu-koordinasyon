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

        // Normal kayıt: kullanıcı kendine sadece Afetzede ya da Gönüllü rolü seçebilir
        public void KayitOl(string adSoyad, string email, string parola, string rol)
        {
            if (rol != "Afetzede" && rol != "Gonullu")
                throw new ArgumentException("Geçersiz rol.");
            Olustur(adSoyad, email, parola, rol);
        }

        // İLK KURULUM (Hafta 14): sistemde hiç koordinatör yoksa ilk koordinatör hesabı oluşturulur.
        // Depoda varsayılan kullanıcı/parola tutulmaz; koordinatör varken bu yolla yenisi açılamaz.
        public void IlkKoordinatoruOlustur(string adSoyad, string email, string parola)
        {
            if (KoordinatorVarMi())
                throw new InvalidOperationException("Sistemde zaten bir koordinatör hesabı var.");
            Olustur(adSoyad, email, parola, "Koordinator");
        }

        public bool KoordinatorVarMi()
        {
            using (var db = new AfetDbContext())
                return db.Kullanicilar.Any(k => k.Rol.Trim() == "Koordinator");
        }

        private void Olustur(string adSoyad, string email, string parola, string rol)
        {
            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Ad soyad ve e-posta zorunludur.");

            email = email.Trim().ToLowerInvariant();
            int at = email.IndexOf('@');
            if (at < 1 || email.IndexOf('.', at) < at + 2 || email.EndsWith(".") || email.Contains(" "))
                throw new ArgumentException("Geçerli bir e-posta adresi girin (ör. ad@ornek.com).");
            if (parola == null || parola.Length < 8)
                throw new ArgumentException("Parola en az 8 karakter olmalıdır.");

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