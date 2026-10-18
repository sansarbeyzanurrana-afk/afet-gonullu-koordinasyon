namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KullaniciEklendi : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Kullanicilar",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdSoyad = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        ParolaHash = c.String(nullable: false, maxLength: 200),
                        Rol = c.String(nullable: false, maxLength: 20),
                        HataliGiris = c.Int(nullable: false),
                        KilitBitis = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Kullanicilar", new[] { "Email" });
            DropTable("dbo.Kullanicilar");
        }
    }
}
