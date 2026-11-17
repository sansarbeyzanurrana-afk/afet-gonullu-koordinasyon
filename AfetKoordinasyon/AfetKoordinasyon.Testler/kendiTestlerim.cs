using AfetKoordinasyon.Is;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    [TestClass]
    public class KendiTestlerim
    {
        // İptal edilen bir ihtiyaç tekrar eşleştirilemez
        [TestMethod]
        public void IptalEdilenIhtiyac_TekrarEslestirilemez()
        {
            bool sonuc = DurumServisi.GecisGecerliMi("İptal", "Eşleşti");
            Assert.IsFalse(sonuc);
        }

        // 4 talepten 1'i karşılandıysa oran %25 olmalı
        [TestMethod]
        public void DortTalepteBirKarsilanan_Yuzde25()
        {
            string sonuc = RaporServisi.OranMetni(1, 4);
            Assert.AreEqual("%25 (1/4)", sonuc);
        }
    }
}
