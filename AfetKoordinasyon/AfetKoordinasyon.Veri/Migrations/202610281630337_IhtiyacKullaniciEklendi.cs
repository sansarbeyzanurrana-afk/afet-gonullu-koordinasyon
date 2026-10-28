namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IhtiyacKullaniciEklendi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ihtiyac", "KullaniciId", c => c.Int());
            CreateIndex("dbo.Ihtiyac", "KullaniciId");
            AddForeignKey("dbo.Ihtiyac", "KullaniciId", "dbo.Kullanicilar", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ihtiyac", "KullaniciId", "dbo.Kullanicilar");
            DropIndex("dbo.Ihtiyac", new[] { "KullaniciId" });
            DropColumn("dbo.Ihtiyac", "KullaniciId");
        }
    }
}
