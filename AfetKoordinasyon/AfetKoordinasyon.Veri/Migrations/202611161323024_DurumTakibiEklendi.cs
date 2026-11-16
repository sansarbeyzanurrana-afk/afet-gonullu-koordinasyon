namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DurumTakibiEklendi : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DurumGecmisi",
                c => new
                    {
                        DurumGecmisiId = c.Int(nullable: false, identity: true),
                        IhtiyacId = c.Int(nullable: false),
                        EskiDurum = c.String(maxLength: 30),
                        YeniDurum = c.String(nullable: false, maxLength: 30),
                        Tarih = c.DateTime(nullable: false),
                        KullaniciId = c.Int(),
                        Not = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.DurumGecmisiId)
                .ForeignKey("dbo.Ihtiyac", t => t.IhtiyacId)
                .ForeignKey("dbo.Kullanicilar", t => t.KullaniciId)
                .Index(t => t.IhtiyacId)
                .Index(t => t.KullaniciId);
            
            AddColumn("dbo.Ihtiyac", "SatirSurumu", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.Eslesme", "Aktif", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Eslesme", "BitisTarihi", c => c.DateTime());

            // 1) Mevcut veriyi düzenle: sadece "Eşleşti" durumundaki ihtiyaçların eşleşmesi aktif kalsın
            Sql(@"UPDATE e SET e.Aktif = 0, e.BitisTarihi = GETDATE()
                  FROM dbo.Eslesme e
                  JOIN dbo.Ihtiyac i ON i.IhtiyacId = e.IhtiyacId
                  JOIN dbo.Durum d ON d.DurumId = i.DurumId
                  WHERE d.Ad <> N'Eşleşti'");

            // 2) Aynı ihtiyacın ya da aynı gönüllünün birden fazla aktif eşleşmesi varsa en yenisi kalsın
            Sql(@";WITH x AS (SELECT EslesmeId, ROW_NUMBER() OVER (PARTITION BY IhtiyacId ORDER BY Tarih DESC, EslesmeId DESC) AS rn
                             FROM dbo.Eslesme WHERE Aktif = 1)
                  UPDATE e SET e.Aktif = 0, e.BitisTarihi = GETDATE()
                  FROM dbo.Eslesme e JOIN x ON x.EslesmeId = e.EslesmeId WHERE x.rn > 1");
            Sql(@";WITH x AS (SELECT EslesmeId, ROW_NUMBER() OVER (PARTITION BY GonulluId ORDER BY Tarih DESC, EslesmeId DESC) AS rn
                             FROM dbo.Eslesme WHERE Aktif = 1)
                  UPDATE e SET e.Aktif = 0, e.BitisTarihi = GETDATE()
                  FROM dbo.Eslesme e JOIN x ON x.EslesmeId = e.EslesmeId WHERE x.rn > 1");

            // 3) ÇİFT ATAMA KURALI (veritabanı seviyesinde):
            //    bir ihtiyacın ve bir gönüllünün aynı anda yalnızca BİR aktif eşleşmesi olabilir
            Sql("CREATE UNIQUE INDEX UX_Eslesme_AktifIhtiyac ON dbo.Eslesme(IhtiyacId) WHERE Aktif = 1");
            Sql("CREATE UNIQUE INDEX UX_Eslesme_AktifGonullu ON dbo.Eslesme(GonulluId) WHERE Aktif = 1");

            // 4) Geçmiş takibi başlamadan önceki kayıtlar için başlangıç satırı
            Sql(@"INSERT INTO dbo.DurumGecmisi (IhtiyacId, EskiDurum, YeniDurum, Tarih, KullaniciId, [Not])
                  SELECT i.IhtiyacId, NULL, d.Ad, i.Tarih, i.KullaniciId, N'Başlangıç kaydı (geçmiş takibi bu tarihten önce yoktu)'
                  FROM dbo.Ihtiyac i JOIN dbo.Durum d ON d.DurumId = i.DurumId");
        }
        
        public override void Down()
        {
            Sql("DROP INDEX UX_Eslesme_AktifGonullu ON dbo.Eslesme");
            Sql("DROP INDEX UX_Eslesme_AktifIhtiyac ON dbo.Eslesme");
            DropForeignKey("dbo.DurumGecmisi", "KullaniciId", "dbo.Kullanicilar");
            DropForeignKey("dbo.DurumGecmisi", "IhtiyacId", "dbo.Ihtiyac");
            DropIndex("dbo.DurumGecmisi", new[] { "KullaniciId" });
            DropIndex("dbo.DurumGecmisi", new[] { "IhtiyacId" });
            DropColumn("dbo.Eslesme", "BitisTarihi");
            DropColumn("dbo.Eslesme", "Aktif");
            DropColumn("dbo.Ihtiyac", "SatirSurumu");
            DropTable("dbo.DurumGecmisi");
        }
    }
}
