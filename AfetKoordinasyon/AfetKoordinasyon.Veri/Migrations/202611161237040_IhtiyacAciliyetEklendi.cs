namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IhtiyacAciliyetEklendi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ihtiyac", "AciliyetSkoru", c => c.Int());
            AddColumn("dbo.Ihtiyac", "AiGerekce", c => c.String(maxLength: 300));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ihtiyac", "AiGerekce");
            DropColumn("dbo.Ihtiyac", "AciliyetSkoru");
        }
    }
}
