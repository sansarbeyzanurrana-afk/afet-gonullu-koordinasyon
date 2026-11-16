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
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.ToString(), "Hata");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MessageBox.Show(e.ExceptionObject.ToString(), "Hata");

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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
                MessageBox.Show(ex.ToString(), "Hata");
            }
        }
    }
}
