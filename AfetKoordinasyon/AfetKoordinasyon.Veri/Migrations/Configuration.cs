namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using AfetKoordinasyon.Veri.Varliklar;

    internal sealed class Configuration : DbMigrationsConfiguration<AfetDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AfetDbContext context)
        {
            // Sentetik veri: gerçek kişilere ait değildir (KVKK)
            context.Durumlar.AddOrUpdate(d => d.Ad,
                new Durum { Ad = "Bekliyor" },
                new Durum { Ad = "Eşleşti" },
                new Durum { Ad = "Tamamlandı" });

            context.Kategoriler.AddOrUpdate(k => k.Ad,
               new Kategori { Ad = "Gıda" },
               new Kategori { Ad = "Su" },
               new Kategori { Ad = "Barınma" },
               new Kategori { Ad = "Sağlık" },
               new Kategori { Ad = "Giyim" },
               new Kategori { Ad = "Hijyen" });
            context.SaveChanges();

            var gidaKat = context.Kategoriler.Single(k => k.Ad == "Gıda");
            var barinmaKat = context.Kategoriler.Single(k => k.Ad == "Barınma");






            context.Konumlar.AddOrUpdate(k => new { k.Il, k.Ilce },
                new Konum { Il = "Bingöl", Ilce = "Merkez", Mahalle = "Örnek Mah." },
                new Konum { Il = "Bingöl", Ilce = "Genç", Mahalle = "Deneme Mah." });
            context.SaveChanges();

            var merkez = context.Konumlar.Single(k => k.Ilce == "Merkez");
            var genc = context.Konumlar.Single(k => k.Ilce == "Genç");
            var bekliyor = context.Durumlar.Single(d => d.Ad == "Bekliyor");
            var eslesti = context.Durumlar.Single(d => d.Ad == "Eşleşti");

            context.Gonulluler.AddOrUpdate(g => g.Telefon,
                new Gonullu { AdSoyad = "Gönüllü Bir", Telefon = "05550000001", Beceri = "İlk yardım", KonumId = merkez.KonumId },
                new Gonullu { AdSoyad = "Gönüllü İki", Telefon = "05550000002", Beceri = "Lojistik", KonumId = genc.KonumId });

            context.Ihtiyaclar.AddOrUpdate(i => i.Aciklama,
                new Ihtiyac { Tur = "Gıda", Aciklama = "Kuru gıda kolisi", Miktar = 20, Tarih = new DateTime(2026, 10, 1), KonumId = merkez.KonumId, DurumId = eslesti.DurumId ,KategoriId = gidaKat.Id },
                new Ihtiyac { Tur = "Barınma", Aciklama = "Çadır", Miktar = 5, Tarih = new DateTime(2026, 10, 2), KonumId = genc.KonumId, DurumId = bekliyor.DurumId , KategoriId = barinmaKat.Id });
            context.SaveChanges();

            var g1 = context.Gonulluler.Single(g => g.Telefon == "05550000001");
            var gida = context.Ihtiyaclar.Single(i => i.Aciklama == "Kuru gıda kolisi");

            context.Eslesmeler.AddOrUpdate(e => new { e.IhtiyacId, e.GonulluId },
                new Eslesme { IhtiyacId = gida.IhtiyacId, GonulluId = g1.GonulluId, Tarih = new DateTime(2026, 10, 3) });
            context.SaveChanges();
        }
    }
}