namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KonumKoordinatEklendi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Konum", "Enlem", c => c.Double());
            AddColumn("dbo.Konum", "Boylam", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Konum", "Boylam");
            DropColumn("dbo.Konum", "Enlem");
        }
    }
}
