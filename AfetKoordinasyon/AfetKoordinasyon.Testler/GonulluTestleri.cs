using System;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri.Varliklar;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ (Hafta 15): gönüllü ekleme kuralları veritabanına gitmeden kontrol edilir
    [TestClass]
    public class GonulluTestleri
    {
        private readonly GonulluServisi servis = new GonulluServisi();

        [TestCleanup]
        public void Temizle() { Oturum.Cikis(); }

        [TestMethod]
        public void KoordinatorOlmayan_GonulluEkleyemez()
        {
            Oturum.Baslat(new Kullanici { Id = 0, AdSoyad = "Test", Email = "t@ornek.test", Rol = "Afetzede" });
            Assert.ThrowsException<UnauthorizedAccessException>(() =>
                servis.Ekle(new Gonullu { AdSoyad = "Test Gönüllü", Telefon = "05990000000", KonumId = 1 }));
        }

        [DataTestMethod]
        [DataRow("5321234567", DisplayName = "10 hane")]
        [DataRow("15321234567", DisplayName = "05 ile başlamıyor")]
        [DataRow("0532123456a", DisplayName = "Harf içeriyor")]
        [DataRow("", DisplayName = "Boş")]
        public void GecersizTelefon_Reddedilir(string telefon)
        {
            Oturum.Baslat(new Kullanici { Id = 0, AdSoyad = "Test", Email = "t@ornek.test", Rol = "Koordinator" });
            var hata = Assert.ThrowsException<ArgumentException>(() =>
                servis.Ekle(new Gonullu { AdSoyad = "Test Gönüllü", Telefon = telefon, KonumId = 1 }));
            StringAssert.Contains(hata.Message, "Telefon");
        }
    }
}
