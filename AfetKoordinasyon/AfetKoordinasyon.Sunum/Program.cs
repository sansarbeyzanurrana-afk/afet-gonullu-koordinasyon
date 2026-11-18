using AfetKoordinasyon.Is;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AfetKoordinasyon.Sunum
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Beklenmeyen hatalarda teknik yığın yerine anlaşılır mesaj gösterilir
            Application.ThreadException += (s, e) =>
                MessageBox.Show("Beklenmeyen bir hata oluştu:\n\n" + e.Exception.GetBaseException().Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MessageBox.Show("Beklenmeyen bir hata oluştu:\n\n" + ((e.ExceptionObject as Exception)?.GetBaseException().Message ?? e.ExceptionObject.ToString()),
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // DAĞITIM (Hafta 14): veritabanı yoksa oluşturulur, eksik migration'lar uygulanır
                try
                {
                    new KurulumServisi().VeritabaniniHazirla();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Veritabanı hazırlanamadı.\n\n" +
                        "Kontrol edin:\n" +
                        "• SQL Server Express LocalDB kurulu mu? (Visual Studio Installer → \".NET masaüstü geliştirme\" ile gelir)\n" +
                        "• App.config'deki bağlantı dizesi (AfetDb) doğru mu?\n\n" +
                        "Ayrıntı: " + ex.GetBaseException().Message,
                        "Kurulum hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // İLK KURULUM: hiç koordinatör yoksa önce koordinatör hesabı oluşturulur
                if (!new KullaniciServisi().KoordinatorVarMi())
                {
                    MessageBox.Show(
                        "Uygulama ilk kez çalıştırılıyor.\nDevam etmek için bir koordinatör hesabı oluşturun.",
                        "İlk kurulum", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var kurulum = new KayitForm(ilkKurulum: true))
                    {
                        if (kurulum.ShowDialog() != DialogResult.OK)
                            return;
                    }
                }

                // Önce giriş formu açılır; giriş başarılı olmazsa program kapanır
                using (var giris = new GirisForm())
                {
                    if (giris.ShowDialog() != DialogResult.OK)
                        return;
                }

                // Giriş başarılıysa ana ekran açılır
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen bir hata oluştu:\n\n" + ex.GetBaseException().Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
