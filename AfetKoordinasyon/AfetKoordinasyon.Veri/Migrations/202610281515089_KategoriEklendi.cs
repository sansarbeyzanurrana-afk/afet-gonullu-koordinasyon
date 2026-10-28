namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KategoriEklendi : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Kategori",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ad = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            Sql("INSERT INTO dbo.Kategori (Ad) VALUES (N'Gıda')");
            AddColumn("dbo.Ihtiyac", "KategoriId", c => c.Int(nullable: false, defaultValue: 1));
            CreateIndex("dbo.Ihtiyac", "KategoriId");
            AddForeignKey("dbo.Ihtiyac", "KategoriId", "dbo.Kategori", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ihtiyac", "KategoriId", "dbo.Kategori");
            DropIndex("dbo.Ihtiyac", new[] { "KategoriId" });
            DropColumn("dbo.Ihtiyac", "KategoriId");
            DropTable("dbo.Kategori");
        }
    }
}
