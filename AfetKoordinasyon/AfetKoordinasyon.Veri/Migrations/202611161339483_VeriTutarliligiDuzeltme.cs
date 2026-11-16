namespace AfetKoordinasyon.Veri.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    // Tek seferlik VERİ DÜZELTME: eski Seed'in neden olduğu tutarsızlıkları giderir.
    // Şema değişmez; sadece veri düzeltilir ve her düzeltme geçmişe not olarak yazılır.
    public partial class VeriTutarliligiDuzeltme : DbMigration
    {
        public override void Up()
        {
            // 1) Durumu "Eşleşti"/"Yolda" olmayan ama aktif eşleşmesi olan ihtiyaçlar:
            //    önce geçmişe not düş, sonra eşleşmeyi kapat
            Sql(@"INSERT INTO dbo.DurumGecmisi (IhtiyacId, EskiDurum, YeniDurum, Tarih, KullaniciId, [Not])
                  SELECT i.IhtiyacId, d.Ad, d.Ad, GETDATE(), NULL,
                         N'Veri düzeltme: durumla uyuşmayan aktif eşleşme kapatıldı (' + g.AdSoyad + N').'
                  FROM dbo.Eslesme e
                  JOIN dbo.Ihtiyac i ON i.IhtiyacId = e.IhtiyacId
                  JOIN dbo.Durum d ON d.DurumId = i.DurumId
                  JOIN dbo.Gonullu g ON g.GonulluId = e.GonulluId
                  WHERE e.Aktif = 1 AND d.Ad NOT IN (N'Eşleşti', N'Yolda')");

            Sql(@"UPDATE e SET e.Aktif = 0, e.BitisTarihi = GETDATE()
                  FROM dbo.Eslesme e
                  JOIN dbo.Ihtiyac i ON i.IhtiyacId = e.IhtiyacId
                  JOIN dbo.Durum d ON d.DurumId = i.DurumId
                  WHERE e.Aktif = 1 AND d.Ad NOT IN (N'Eşleşti', N'Yolda')");

            // 2) Durumu "Eşleşti"/"Yolda" olup aktif eşleşmesi OLMAYAN ihtiyaçlar: "Bekliyor"a al
            Sql(@"INSERT INTO dbo.DurumGecmisi (IhtiyacId, EskiDurum, YeniDurum, Tarih, KullaniciId, [Not])
                  SELECT i.IhtiyacId, d.Ad, N'Bekliyor', GETDATE(), NULL,
                         N'Veri düzeltme: atanmış gönüllü bulunamadığı için Bekliyor durumuna alındı.'
                  FROM dbo.Ihtiyac i
                  JOIN dbo.Durum d ON d.DurumId = i.DurumId
                  WHERE d.Ad IN (N'Eşleşti', N'Yolda')
                    AND NOT EXISTS (SELECT 1 FROM dbo.Eslesme e WHERE e.IhtiyacId = i.IhtiyacId AND e.Aktif = 1)");

            Sql(@"UPDATE i SET i.DurumId = (SELECT DurumId FROM dbo.Durum WHERE Ad = N'Bekliyor')
                  FROM dbo.Ihtiyac i
                  JOIN dbo.Durum d ON d.DurumId = i.DurumId
                  WHERE d.Ad IN (N'Eşleşti', N'Yolda')
                    AND NOT EXISTS (SELECT 1 FROM dbo.Eslesme e WHERE e.IhtiyacId = i.IhtiyacId AND e.Aktif = 1)");

            // 3) Gönüllülerin müsaitlik bilgisini aktif eşleşmelere göre yeniden hesapla
            Sql(@"UPDATE g SET g.MusaitMi = CASE WHEN EXISTS
                         (SELECT 1 FROM dbo.Eslesme e WHERE e.GonulluId = g.GonulluId AND e.Aktif = 1)
                       THEN 0 ELSE 1 END
                  FROM dbo.Gonullu g");
        }
        
        public override void Down()
        {
            // Veri düzeltmesi geri alınmaz (eski hatalı duruma dönmenin anlamı yok).
        }
    }
}
