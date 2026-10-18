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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            while (true)
            {
                using (var giris = new GirisForm())
                {
                    if (giris.ShowDialog() != DialogResult.OK)
                        return; // giriş ekranı kapatıldı → uygulama kapanır
                }

                Application.Run(new Form1());

                if (Oturum.Aktif)
                    return; // Form1 X ile kapatıldı → uygulama kapanır
                            // Çıkış yapıldıysa döngü başa döner, giriş ekranı tekrar açılır
            }
        }
    }
}
