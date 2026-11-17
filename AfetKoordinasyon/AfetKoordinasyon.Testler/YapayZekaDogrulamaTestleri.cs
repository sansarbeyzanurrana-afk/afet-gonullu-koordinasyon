using System.Collections.Generic;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri.Varliklar;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // BİRİM TESTİ: yapay zekânın cevabı körü körüne kullanılmıyor; doğrulama ve yedek kurallar test edilir.
    // Gerçek API'ye istek atılmaz (hızlı, ücretsiz, internet gerektirmez).
    [TestClass]
    public class YapayZekaDogrulamaTestleri
    {
        private static readonly List<Kategori> Kategoriler = new List<Kategori>
        {
            new Kategori { Id = 1, Ad = "Gıda" },
            new Kategori { Id = 2, Ad = "Su" },
            new Kategori { Id = 3, Ad = "Barınma" },
            new Kategori { Id = 4, Ad = "Sağlık" },
            new Kategori { Id = 5, Ad = "Giyim" },
            new Kategori { Id = 6, Ad = "Hijyen" },
        };

        private static OncelikSonucu Dogrula(string cevap, out string hata)
        {
            return IhtiyacOnceliklendirmeServisi.Dogrula(cevap, Kategoriler, out hata);
        }

        // ---------------- AI cevabının doğrulanması ----------------

        [TestMethod]
        public void GecerliCevap_KabulEdilir()
        {
            string hata;
            var s = Dogrula("{\"kategori\":\"Sağlık\",\"aciliyet\":5,\"gerekce\":\"Yaralı var.\"}", out hata);
            Assert.IsNull(hata);
            Assert.AreEqual(4, s.KategoriId);
            Assert.AreEqual(5, s.Aciliyet);
        }

        [TestMethod]
        public void KategoriBuyukHarfle_TurkceKurallaEslesir()
        {
            string hata;
            var s = Dogrula("{\"kategori\":\"SAĞLIK\",\"aciliyet\":\"4\"}", out hata);
            Assert.IsNotNull(s, hata);
            Assert.AreEqual("Sağlık", s.KategoriAd);
            Assert.AreEqual(4, s.Aciliyet);     // "4" metin olarak gelse de kabul
        }

        [TestMethod]
        public void KodBlogundakiJson_Temizlenir()
        {
            string hata;
            var s = Dogrula("```json\n{\"kategori\":\"Su\",\"aciliyet\":3}\n```", out hata);
            Assert.IsNotNull(s, hata);
            Assert.AreEqual("Su", s.KategoriAd);
        }

        // SINIR DEĞERLERİ: 1 ve 5 geçerli, 0 ve 6 geçersiz
        [DataTestMethod]
        [DataRow("0", false)]
        [DataRow("1", true)]
        [DataRow("5", true)]
        [DataRow("6", false)]
        [DataRow("4.5", false)]
        [DataRow("\"yüksek\"", false)]
        public void Aciliyet_SadeceBirIleBesArasiTamSayi(string aciliyet, bool gecerliMi)
        {
            string hata;
            var s = Dogrula("{\"kategori\":\"Gıda\",\"aciliyet\":" + aciliyet + "}", out hata);
            Assert.AreEqual(gecerliMi, s != null, "aciliyet=" + aciliyet + " hata=" + hata);
        }

        [DataTestMethod]
        [DataRow("{\"kategori\":\"Elektronik\",\"aciliyet\":3}", DisplayName = "Listede olmayan kategori")]
        [DataRow("{\"aciliyet\":3}", DisplayName = "Kategori alanı eksik")]
        [DataRow("Bu ihtiyaç acil görünüyor.", DisplayName = "JSON değil")]
        [DataRow("", DisplayName = "Boş cevap")]
        [DataRow(null, DisplayName = "Null cevap")]
        public void HataliCevap_ReddedilirVeNedeniYazilir(string cevap)
        {
            string hata;
            var s = Dogrula(cevap, out hata);
            Assert.IsNull(s);
            Assert.IsFalse(string.IsNullOrEmpty(hata));
        }

        [TestMethod]
        public void UzunGerekce_300KaraktereKisaltilir()
        {
            string hata;
            var uzun = new string('x', 400);
            var s = Dogrula("{\"kategori\":\"Gıda\",\"aciliyet\":2,\"gerekce\":\"" + uzun + "\"}", out hata);
            Assert.AreEqual(300, s.Gerekce.Length);   // veritabanındaki sütun 300 karakter
        }

        // ---------------- AI çalışmazsa: kural tabanlı yedek ----------------

        [TestMethod]
        public void Yedek_HayatiKelime_Aciliyet5()
        {
            var s = IhtiyacOnceliklendirmeServisi.KuralTabanliDegerlendir("Enkaz altında yaralı var", Kategoriler);
            Assert.AreEqual(5, s.Aciliyet);
            Assert.AreEqual("Sağlık", s.KategoriAd);
        }

        [TestMethod]
        public void Yedek_IlacKelimesi_GidaDegilSaglikSayilir()
        {
            // SINIR DURUMU: "ilaç" kelimesinin içinde gıda anahtar kelimesi "aç" geçiyor
            var s = IhtiyacOnceliklendirmeServisi.KuralTabanliDegerlendir("Annem için ilaç lazım", Kategoriler);
            Assert.AreEqual("Sağlık", s.KategoriAd);
            Assert.AreEqual(4, s.Aciliyet);
        }

        [TestMethod]
        public void Yedek_BuyukHarfliTurkceMetin_DogruAnlasilir()
        {
            var s = IhtiyacOnceliklendirmeServisi.KuralTabanliDegerlendir("ÇADIR LAZIM, SOKAKTA KALIYORUZ", Kategoriler);
            Assert.AreEqual("Barınma", s.KategoriAd);
        }

        [TestMethod]
        public void Yedek_AnahtarKelimeYoksa_OrtaAciliyetVeKontrolUyarisi()
        {
            var s = IhtiyacOnceliklendirmeServisi.KuralTabanliDegerlendir("Merhaba, yardım edebilir misiniz", Kategoriler);
            Assert.AreEqual(3, s.Aciliyet);
            StringAssert.Contains(s.Gerekce, "kontrol");
        }
    }
}
