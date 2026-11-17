using AfetKoordinasyon.Is;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ: rapor ve bildirimde ekrana yazılan metinlerin hesaplanması
    [TestClass]
    public class RaporVeBildirimTestleri
    {
        [TestMethod]
        public void OranMetni_OranlaBirlikteSayiyiDaGosterir()
        {
            Assert.AreEqual("%33 (5/15)", RaporServisi.OranMetni(5, 15));
            Assert.AreEqual("%100 (15/15)", RaporServisi.OranMetni(15, 15));
            Assert.AreEqual("%0 (0/9)", RaporServisi.OranMetni(0, 9));
        }

        [TestMethod]
        public void OranMetni_PaydaSifirsaSifiraBolmeHatasiVermez()
        {
            // Hiç talep yokken (ya da hepsi iptalken) "%NaN" yerine "-" yazılmalı
            Assert.AreEqual("-", RaporServisi.OranMetni(0, 0));
        }

        [TestMethod]
        public void SureMetni_VeriYoksaBunuSoyler()
        {
            Assert.AreEqual("veri yok", RaporServisi.SureMetni(null, 0));
        }

        [TestMethod]
        public void SureMetni_SaatVeGunOlarakYazilir()
        {
            Assert.AreEqual("0,7 saat (1 kayıt)", RaporServisi.SureMetni(0.7, 1));
            Assert.AreEqual("2,0 gün (2 kayıt)", RaporServisi.SureMetni(48, 2));
        }

        [TestMethod]
        public void BildirimSuresi_DakikaSaatGun()
        {
            Assert.AreEqual("30 dakikadır", BildirimServisi.SureMetni(0.5));
            Assert.AreEqual("5 saattir", BildirimServisi.SureMetni(5.4));
            Assert.AreEqual("3 gündür", BildirimServisi.SureMetni(75));
        }

        // SINIR DURUMU: 59 dakika 57 saniye (yaklaşık 1 saat)
        [TestMethod]
        public void BildirimSuresi_BirSaateCokYakinSure_60DakikaDegil1SaatYazilir()
        {
            Assert.AreEqual("1 saattir", BildirimServisi.SureMetni(0.999));
        }
    }
}
