using AfetKoordinasyon.Is;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ: veritabanı yok, sadece durum makinesi kuralı test edilir.
    [TestClass]
    public class DurumMakinesiTestleri
    {
        [TestMethod]
        public void Bekliyor_EslestiVeIptaleGecebilir()
        {
            Assert.IsTrue(DurumServisi.GecisGecerliMi(DurumAdlari.Bekliyor, DurumAdlari.Eslesti));
            Assert.IsTrue(DurumServisi.GecisGecerliMi(DurumAdlari.Bekliyor, DurumAdlari.Iptal));
        }

        [TestMethod]
        public void Bekliyor_GonulluAtanmadanTamamlanamaz()
        {
            Assert.IsFalse(DurumServisi.GecisGecerliMi(DurumAdlari.Bekliyor, DurumAdlari.Tamamlandi));
            Assert.IsFalse(DurumServisi.GecisGecerliMi(DurumAdlari.Bekliyor, DurumAdlari.Yolda));
        }

        [TestMethod]
        public void Eslesti_YolaCikabilirTamamlanabilirVeGeriAlinabilir()
        {
            Assert.IsTrue(DurumServisi.GecisGecerliMi(DurumAdlari.Eslesti, DurumAdlari.Yolda));
            Assert.IsTrue(DurumServisi.GecisGecerliMi(DurumAdlari.Eslesti, DurumAdlari.Tamamlandi));
            Assert.IsTrue(DurumServisi.GecisGecerliMi(DurumAdlari.Eslesti, DurumAdlari.Bekliyor));
        }

        [TestMethod]
        public void Yolda_TekrarEslestiyeDonemez()
        {
            Assert.IsFalse(DurumServisi.GecisGecerliMi(DurumAdlari.Yolda, DurumAdlari.Eslesti));
        }

        // Son durumlardan hiçbir yere geçilemez
        [DataTestMethod]
        [DataRow("Tamamlandı")]
        [DataRow("İptal")]
        public void SonDurumdan_HicbirDurumaGecilemez(string sonDurum)
        {
            Assert.AreEqual(0, DurumServisi.SonrakiDurumlar(sonDurum).Length);
            Assert.IsFalse(DurumServisi.GecisGecerliMi(sonDurum, DurumAdlari.Bekliyor));
        }

        // SINIR DURUMU: boş, null ya da tanımsız durum adı programı çökertmemeli
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("bekliyor")]      // küçük harf: tanımlı ad değil
        [DataRow("Kayıp")]
        public void TanimsizDurum_GecisYokVeHataVermez(string durum)
        {
            Assert.AreEqual(0, DurumServisi.SonrakiDurumlar(durum).Length);
            Assert.IsFalse(DurumServisi.GecisGecerliMi(durum, DurumAdlari.Eslesti));
        }
    }
}
