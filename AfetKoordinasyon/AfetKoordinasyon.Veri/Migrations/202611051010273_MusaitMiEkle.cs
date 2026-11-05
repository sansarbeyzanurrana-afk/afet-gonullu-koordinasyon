namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MusaitMiEkle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Gonullu", "MusaitMi", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Gonullu", "MusaitMi");
        }
    }
}
