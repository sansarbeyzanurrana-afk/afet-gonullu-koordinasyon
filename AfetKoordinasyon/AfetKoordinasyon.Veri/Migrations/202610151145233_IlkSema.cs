namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IlkSema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Durum",
                c => new
                    {
                        DurumId = c.Int(nullable: false, identity: true),
                        Ad = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.DurumId);
            
            CreateTable(
                "dbo.Ihtiyac",
                c => new
                    {
                        IhtiyacId = c.Int(nullable: false, identity: true),
                        Tur = c.String(nullable: false, maxLength: 50),
                        Aciklama = c.String(maxLength: 500),
                        Miktar = c.Int(nullable: false),
                        Tarih = c.DateTime(nullable: false),
                        KonumId = c.Int(nullable: false),
                        DurumId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IhtiyacId)
                .ForeignKey("dbo.Durum", t => t.DurumId)
                .ForeignKey("dbo.Konum", t => t.KonumId)
                .Index(t => t.KonumId)
                .Index(t => t.DurumId);
            
            CreateTable(
                "dbo.Eslesme",
                c => new
                    {
                        EslesmeId = c.Int(nullable: false, identity: true),
                        Tarih = c.DateTime(nullable: false),
                        IhtiyacId = c.Int(nullable: false),
                        GonulluId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EslesmeId)
                .ForeignKey("dbo.Gonullu", t => t.GonulluId)
                .ForeignKey("dbo.Ihtiyac", t => t.IhtiyacId)
                .Index(t => t.IhtiyacId)
                .Index(t => t.GonulluId);
            
            CreateTable(
                "dbo.Gonullu",
                c => new
                    {
                        GonulluId = c.Int(nullable: false, identity: true),
                        AdSoyad = c.String(nullable: false, maxLength: 100),
                        Telefon = c.String(nullable: false, maxLength: 15),
                        Beceri = c.String(maxLength: 200),
                        KonumId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GonulluId)
                .ForeignKey("dbo.Konum", t => t.KonumId)
                .Index(t => t.KonumId);
            
            CreateTable(
                "dbo.Konum",
                c => new
                    {
                        KonumId = c.Int(nullable: false, identity: true),
                        Il = c.String(nullable: false, maxLength: 50),
                        Ilce = c.String(nullable: false, maxLength: 50),
                        Mahalle = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.KonumId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Eslesme", "IhtiyacId", "dbo.Ihtiyac");
            DropForeignKey("dbo.Ihtiyac", "KonumId", "dbo.Konum");
            DropForeignKey("dbo.Gonullu", "KonumId", "dbo.Konum");
            DropForeignKey("dbo.Eslesme", "GonulluId", "dbo.Gonullu");
            DropForeignKey("dbo.Ihtiyac", "DurumId", "dbo.Durum");
            DropIndex("dbo.Gonullu", new[] { "KonumId" });
            DropIndex("dbo.Eslesme", new[] { "GonulluId" });
            DropIndex("dbo.Eslesme", new[] { "IhtiyacId" });
            DropIndex("dbo.Ihtiyac", new[] { "DurumId" });
            DropIndex("dbo.Ihtiyac", new[] { "KonumId" });
            DropTable("dbo.Konum");
            DropTable("dbo.Gonullu");
            DropTable("dbo.Eslesme");
            DropTable("dbo.Ihtiyac");
            DropTable("dbo.Durum");
        }
    }
}
