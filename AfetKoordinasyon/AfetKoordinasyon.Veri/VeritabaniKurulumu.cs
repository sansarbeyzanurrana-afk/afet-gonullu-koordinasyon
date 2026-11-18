using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace AfetKoordinasyon.Veri
{
    // DAĞITIM (Hafta 14): Uygulama açılırken veritabanını kendisi hazırlar.
    // - Veritabanı yoksa: tüm migration'larla oluşturur ve örnek verileri (Seed) ekler.
    // - Varsa: sadece henüz uygulanmamış migration'ları uygular; eksik yoksa hiçbir şey yapmaz.
    // Böylece exe'yi alan kişinin Package Manager Console'da Update-Database çalıştırması gerekmez.
    public static class VeritabaniKurulumu
    {
        public static void Hazirla()
        {
            // Şemayı sadece migration'lar yönetsin (EF'in kendi otomatik oluşturması kapalı)
            Database.SetInitializer<AfetDbContext>(null);

            var migrator = new DbMigrator(new Migrations.Configuration());
            if (migrator.GetPendingMigrations().Any())
                migrator.Update();
        }
    }
}
