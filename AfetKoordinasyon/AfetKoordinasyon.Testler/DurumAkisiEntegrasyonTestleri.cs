using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AfetKoordinasyon.Testler
{
    // ENTEGRASYON TESTİ: İş katmanı + Entity Framework + gerçek SQL Server birlikte çalışır.
    // ÖNEMLİ: Testler AYRI bir veritabanında çalışır (App.config: AfetKoordinasyonTestDb).
    // Asıl veritabanındaki (AfetKoordinasyonDb) kayıtlara dokunulmaz.
    [TestClass]
    public class DurumAkisiEntegrasyonTestleri
    {
        private static int testKullaniciId;
        private static readonly Random rnd = new Random();
        private readonly DurumServisi servis = new DurumServisi();

        [ClassInitialize]
        public static void TestVeritabaniniHazirla(TestContext context)
        {
            // Test veritabanını tüm migration'larla oluştur/güncelle (Seed de çalışır)
            Database.SetInitializer<AfetDbContext>(null);
            new DbMigrator(new Veri.Migrations.Configuration()).Update();

            using (var db = new AfetDbContext())
            {
                var k = db.Kullanicilar.FirstOrDefault(x => x.Email == "test-koordinator@ornek.test");
                if (k == null)
                {
                    k = new Kullanici { AdSoyad = "Test Koordinatör", Email = "test-koordinator@ornek.test", ParolaHash = "-", Rol = "Koordinator" };
                    db.Kullanicilar.Add(k);
                    db.SaveChanges();
                }
                testKullaniciId = k.Id;
            }
        }

        [TestInitialize]
        public void KoordinatorOlarakGiris()
        {
            using (var db = new AfetDbContext())
                Oturum.Baslat(db.Kullanicilar.Find(testKullaniciId));
        }

        [TestCleanup]
        public void Cikis()
        {
            Oturum.Cikis();
        }

        // ---------------- Testler ----------------

        [TestMethod]
        public void Ata_BekleyenIhtiyac_EslestiOlurVeGecmiseYazilir()
        {
            int ihtiyac = YeniIhtiyac(), gonullu = YeniGonullu();

            servis.Ata(ihtiyac, gonullu);

            using (var db = new AfetDbContext())
            {
                Assert.AreEqual(DurumAdlari.Eslesti, DurumAdi(db, ihtiyac));
                Assert.IsFalse(db.Gonulluler.Find(gonullu).MusaitMi, "Atanan gönüllü müsait görünmemeli");
                Assert.AreEqual(1, db.Eslesmeler.Count(e => e.IhtiyacId == ihtiyac && e.Aktif));

                var g = db.DurumGecmisleri.Single(x => x.IhtiyacId == ihtiyac);
                Assert.AreEqual(DurumAdlari.Bekliyor, g.EskiDurum);
                Assert.AreEqual(DurumAdlari.Eslesti, g.YeniDurum);
                Assert.AreEqual(testKullaniciId, g.KullaniciId, "İşlemi kimin yaptığı kaydedilmeli");
            }
        }

        [TestMethod]
        public void UctanUca_AtaYolaCiktiTamamla_GonulluTekrarMusaitOlur()
        {
            int ihtiyac = YeniIhtiyac(), gonullu = YeniGonullu();

            servis.Ata(ihtiyac, gonullu);
            servis.YolaCikti(ihtiyac);
            servis.Tamamla(ihtiyac, null, "Teslim edildi");

            using (var db = new AfetDbContext())
            {
                Assert.AreEqual(DurumAdlari.Tamamlandi, DurumAdi(db, ihtiyac));
                Assert.IsTrue(db.Gonulluler.Find(gonullu).MusaitMi, "İş bitince gönüllü yeniden müsait olmalı");

                var e = db.Eslesmeler.Single(x => x.IhtiyacId == ihtiyac);
                Assert.IsFalse(e.Aktif);
                Assert.IsNotNull(e.BitisTarihi);

                var gecisler = db.DurumGecmisleri.Where(x => x.IhtiyacId == ihtiyac)
                                 .OrderBy(x => x.DurumGecmisiId).Select(x => x.YeniDurum).ToList();
                CollectionAssert.AreEqual(new[] { "Eşleşti", "Yolda", "Tamamlandı" }, gecisler);
            }
        }

        [TestMethod]
        public void CiftAtama_AyniGonulluIkinciIseAtanamaz()
        {
            int ihtiyac1 = YeniIhtiyac(), ihtiyac2 = YeniIhtiyac(), gonullu = YeniGonullu();
            servis.Ata(ihtiyac1, gonullu);

            var hata = Assert.ThrowsException<InvalidOperationException>(() => servis.Ata(ihtiyac2, gonullu));
            StringAssert.Contains(hata.Message, "müsait değil");

            using (var db = new AfetDbContext())
            {
                Assert.AreEqual(DurumAdlari.Bekliyor, DurumAdi(db, ihtiyac2), "İkinci ihtiyaç değişmemeli");
                Assert.AreEqual(1, db.Eslesmeler.Count(e => e.GonulluId == gonullu && e.Aktif));
            }
        }

        [TestMethod]
        public void CiftAtama_VeritabaniIndeksiDeEngeller()
        {
            // Kodu atlayıp doğrudan veritabanına ikinci aktif eşleşme yazmayı dene:
            // benzersiz indeks (UX_Eslesme_AktifGonullu) son savunma hattıdır.
            int ihtiyac1 = YeniIhtiyac(), ihtiyac2 = YeniIhtiyac(), gonullu = YeniGonullu();
            servis.Ata(ihtiyac1, gonullu);

            using (var db = new AfetDbContext())
            {
                db.Eslesmeler.Add(new Eslesme { IhtiyacId = ihtiyac2, GonulluId = gonullu, Tarih = DateTime.Now, Aktif = true });
                Assert.ThrowsException<System.Data.Entity.Infrastructure.DbUpdateException>(() => db.SaveChanges());
            }
        }

        [TestMethod]
        public void BekleyenIhtiyac_GonulluAtanmadanTamamlanamaz()
        {
            int ihtiyac = YeniIhtiyac();

            var hata = Assert.ThrowsException<InvalidOperationException>(() => servis.Tamamla(ihtiyac));
            StringAssert.Contains(hata.Message, "geçirilemez");

            using (var db = new AfetDbContext())
            {
                Assert.AreEqual(DurumAdlari.Bekliyor, DurumAdi(db, ihtiyac));
                Assert.AreEqual(0, db.DurumGecmisleri.Count(x => x.IhtiyacId == ihtiyac), "Reddedilen işlem geçmişe yazılmamalı");
            }
        }

        [TestMethod]
        public void Iptal_AktifAtamayiKapatirVeGonulluyuSerbestBirakir()
        {
            int ihtiyac = YeniIhtiyac(), gonullu = YeniGonullu();
            servis.Ata(ihtiyac, gonullu);

            servis.IptalEt(ihtiyac, "Talep sahibi başka yerden yardım aldı");

            using (var db = new AfetDbContext())
            {
                Assert.AreEqual(DurumAdlari.Iptal, DurumAdi(db, ihtiyac));
                Assert.IsTrue(db.Gonulluler.Find(gonullu).MusaitMi);
                Assert.AreEqual(0, db.Eslesmeler.Count(e => e.IhtiyacId == ihtiyac && e.Aktif));
            }
        }

        [TestMethod]
        public void Eszamanlilik_EskiEkrandanYapilanIslemReddedilir()
        {
            // Koordinatör A listeyi açtı (sürümü aldı); bu sırada B gönüllü atadı.
            int ihtiyac = YeniIhtiyac(), gonullu = YeniGonullu();
            byte[] eskiSurum;
            using (var db = new AfetDbContext())
                eskiSurum = db.Ihtiyaclar.AsNoTracking().Single(i => i.IhtiyacId == ihtiyac).SatirSurumu;

            servis.Ata(ihtiyac, gonullu);   // B'nin işlemi

            // A, eski ekrandaki bilgiyle işlem yapmaya çalışır
            var hata = Assert.ThrowsException<InvalidOperationException>(
                () => servis.IptalEt(ihtiyac, "Eski ekrandan iptal denemesi", eskiSurum));
            StringAssert.Contains(hata.Message, "başka biri");

            using (var db = new AfetDbContext())
                Assert.AreEqual(DurumAdlari.Eslesti, DurumAdi(db, ihtiyac), "B'nin işlemi ezilmemeli");
        }

        // ---------------- Yardımcılar (uydurma test verisi) ----------------

        private int YeniIhtiyac()
        {
            using (var db = new AfetDbContext())
            {
                var i = new Ihtiyac
                {
                    Tur = "Test",
                    Aciklama = "Entegrasyon testi kaydı",
                    Miktar = 1,
                    Tarih = DateTime.Now,
                    KonumId = db.Konumlar.First(k => k.Ilce == "Merkez").KonumId,
                    KategoriId = db.Kategoriler.First(k => k.Ad == "Gıda").Id,
                    DurumId = db.Durumlar.First(d => d.Ad == DurumAdlari.Bekliyor).DurumId,
                    KullaniciId = testKullaniciId
                };
                db.Ihtiyaclar.Add(i);
                db.SaveChanges();
                return i.IhtiyacId;
            }
        }

        private int YeniGonullu()
        {
            using (var db = new AfetDbContext())
            {
                var g = new Gonullu
                {
                    AdSoyad = "Test Gönüllü",
                    Telefon = "0599" + rnd.Next(1000000, 9999999),
                    Beceri = "Test",
                    MusaitMi = true,
                    KonumId = db.Konumlar.First(k => k.Ilce == "Merkez").KonumId
                };
                db.Gonulluler.Add(g);
                db.SaveChanges();
                return g.GonulluId;
            }
        }

        private static string DurumAdi(AfetDbContext db, int ihtiyacId)
        {
            return db.Ihtiyaclar.Where(i => i.IhtiyacId == ihtiyacId).Select(i => i.Durum.Ad).Single();
        }
    }
}
