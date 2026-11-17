using System;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri.Varliklar;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ: yetki ve zorunlu not kuralları veritabanına gitmeden önce kontrol edildiği için
    // bu testler veritabanı olmadan çalışır.
    [TestClass]
    public class YetkiVeNotTestleri
    {
        private readonly DurumServisi servis = new DurumServisi();

        private static void GirisYap(string rol)
        {
            Oturum.Baslat(new Kullanici { Id = 0, AdSoyad = "Test Kullanıcı", Email = "test@ornek.test", Rol = rol });
        }

        [TestCleanup]
        public void Temizle()
        {
            Oturum.Cikis();
        }

        [TestMethod]
        public void OturumYokken_AtamaYapilamaz()
        {
            Oturum.Cikis();
            Assert.ThrowsException<UnauthorizedAccessException>(() => servis.Ata(1, 1));
        }

        [TestMethod]
        public void GonulluRolu_IptalEdemez()
        {
            GirisYap("Gonullu");
            Assert.ThrowsException<UnauthorizedAccessException>(() => servis.IptalEt(1, "Yanlış kayıt girilmiş"));
        }

        [TestMethod]
        public void RolKontrolu_BuyukKucukHarfVeBoslukDuyarsiz()
        {
            GirisYap(" koordinator ");
            // Yetki geçer, boş not kuralına takılır: yani hata yetkiden DEĞİL nottan gelmeli
            Assert.ThrowsException<ArgumentException>(() => servis.IptalEt(1, ""));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("    ")]
        [DataRow("abcd")]          // 4 karakter: sınırın bir altı
        [DataRow("  abcd  ")]      // boşluklar sayılmaz, yine 4 karakter
        public void Iptal_KisaVeyaBosNotIleYapilamaz(string not)
        {
            GirisYap("Koordinator");
            var hata = Assert.ThrowsException<ArgumentException>(() => servis.IptalEt(1, not));
            StringAssert.Contains(hata.Message, "en az 5 karakter");
        }

        [TestMethod]
        public void AtamayiKaldir_500KarakterdenUzunNotKabulEdilmez()
        {
            GirisYap("Koordinator");
            var uzunNot = new string('a', 501);
            var hata = Assert.ThrowsException<ArgumentException>(() => servis.AtamayiKaldir(1, uzunNot));
            StringAssert.Contains(hata.Message, "en fazla 500");
        }
    }
}
