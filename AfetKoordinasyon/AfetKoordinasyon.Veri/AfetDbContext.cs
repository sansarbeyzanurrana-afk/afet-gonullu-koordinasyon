using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Veri
{
    public class AfetDbContext : DbContext
    {
        public AfetDbContext() : base("name=AfetDb") { }

        public DbSet<Konum> Konumlar { get; set; }
        public DbSet<Durum> Durumlar { get; set; }
        public DbSet<Gonullu> Gonulluler { get; set; }
        public DbSet<Ihtiyac> Ihtiyaclar { get; set; }
        public DbSet<Eslesme> Eslesmeler { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }

        public DbSet<Kategori> Kategoriler { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }
    }
}