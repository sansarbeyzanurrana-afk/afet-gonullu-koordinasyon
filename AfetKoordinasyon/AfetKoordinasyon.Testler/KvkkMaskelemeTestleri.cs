using AfetKoordinasyon.Is;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ: yapay zekâya gönderilmeden önce kişisel veriler maskelenmeli (KVKK).
    // Testlerdeki numaralar uydurmadır.
    [TestClass]
    public class KvkkMaskelemeTestleri
    {
        private static string Maskele(string metin)
        {
            return IhtiyacOnceliklendirmeServisi.KisiselVeriMaskele(metin);
        }

        [DataTestMethod]
        [DataRow("Ulaşın: 05321234567", DisplayName = "Telefon bitişik")]
        [DataRow("Ulaşın: 0532 123 45 67", DisplayName = "Telefon boşluklu")]
        [DataRow("Ulaşın: 0532-123-45-67", DisplayName = "Telefon tireli")]
        [DataRow("Ulaşın: +90 532 123 45 67", DisplayName = "Telefon ülke kodlu")]
        [DataRow("Ulaşın: (0532) 123 45 67", DisplayName = "Telefon parantezli")]
        public void Telefon_HerYazimdaMaskelenir(string metin)
        {
            var sonuc = Maskele(metin);
            StringAssert.Contains(sonuc, "[TELEFON]");
            Assert.IsFalse(sonuc.Contains("123"), "Numaranın bir kısmı açıkta kaldı: " + sonuc);
        }

        [TestMethod]
        public void Eposta_Maskelenir()
        {
            Assert.AreEqual("Mail: [E-POSTA] adresine yazın", Maskele("Mail: ornek.kisi@ornek.test adresine yazın"));
        }

        [TestMethod]
        public void TcKimlikNo_Maskelenir()
        {
            Assert.AreEqual("TC: [TC]", Maskele("TC: 12345678901"));
        }

        // Kişisel veri olmayan sayılar (miktar, adres no) bozulmamalı
        [TestMethod]
        public void KisiselVeriYoksa_MetinDegismez()
        {
            var metin = "Merkez 3. sokakta 20 koli gıda ve 150 litre su lazım";
            Assert.AreEqual(metin, Maskele(metin));
        }
    }
}
