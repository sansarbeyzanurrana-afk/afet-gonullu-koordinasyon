using System;
using System.Drawing;
using System.Windows.Forms;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{
    public partial class GirisForm : Form
    {
        private readonly KullaniciServisi kullaniciServisi = new KullaniciServisi();

        public GirisForm()
        {
            InitializeComponent();
            GorunumuDuzenle();

            // KULLANILABİLİRLİK (Hafta 14)
            Text = "Giriş - Afet Sonrası İhtiyaç ve Gönüllü Koordinasyon";
            StartPosition = FormStartPosition.CenterScreen;
            AcceptButton = btnGiris;                         // Enter ile giriş yapılır
            txtEmail.AccessibleName = "E-posta";             // ekran okuyucular için alan adları
            txtParola.AccessibleName = "Parola";
            btnKayit.AccessibleDescription = "Yeni kullanıcı kaydı açar";
            Shown += (s, e) => txtEmail.Focus();             // açılınca imleç e-postada
        }

        // GÖRÜNÜM (Hafta 14): solda uygulama adı, sağda beyaz giriş kartı
        private void GorunumuDuzenle()
        {
            SuspendLayout();
            Tema.Uygula(this, Properties.Resources.ArkaPlanGiris);
            Tema.Kontroller(Controls, koyuZemin: false);   // bu kontroller beyaz kartın içine girecek: koyu yazı
            ClientSize = new Size(980, 580);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // Sol taraf: uygulamanın adı ve kısa açıklaması
            var uygulamaAdi = Tema.Baslik("Afet Sonrası\nİhtiyaç ve Gönüllü\nKoordinasyon", 26f, Color.White);
            uygulamaAdi.Location = new Point(60, 150);
            var aciklama = new Label
            {
                Text = "İhtiyaçları topla, gönüllüyle eşleştir,\ndurumu anbean takip et.",
                AutoSize = true, BackColor = Color.Transparent, ForeColor = Tema.AcikYazi,
                Font = Tema.Yazi(12f), Location = new Point(64, 330)
            };
            Controls.Add(uygulamaAdi);
            Controls.Add(aciklama);

            // Sağ taraf: giriş kartı
            label1.Text = "E-posta";
            label2.Text = "Parola";
            Tema.ButonAna(btnGiris);
            Tema.ButonIkincil(btnKayit);
            btnGiris.Height = btnKayit.Height = 42;

            var kart = Tema.Kart(300);
            Tema.KartaEkle(kart, Tema.Baslik("Giriş yap", 18f, Tema.Metin));
            Tema.KartaEkle(kart, new Label { Text = "Hesabınızla devam edin", AutoSize = true, ForeColor = Tema.SolukMetin, Font = Tema.Yazi(10f) });
            Tema.KartaEkle(kart, label1, 18);
            Tema.KartaEkle(kart, txtEmail);
            Tema.KartaEkle(kart, label2, 10);
            Tema.KartaEkle(kart, txtParola);
            Tema.KartaEkle(kart, btnGiris, 22);
            Tema.KartaEkle(kart, new Label { Text = "Hesabınız yok mu?", AutoSize = true, ForeColor = Tema.SolukMetin, Font = Tema.Yazi(9.5f), TextAlign = ContentAlignment.MiddleCenter }, 14);
            Tema.KartaEkle(kart, btnKayit);
            Controls.Add(kart);

            // Kartı sağda dikey ortala (yazı boyutu değişse de doğru yerde dursun)
            Load += (s, e) => kart.Location = new Point(ClientSize.Width - kart.Width - 70, (ClientSize.Height - kart.Height) / 2);
            ResumeLayout(true);
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void btnGiris_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || txtParola.Text.Length == 0)
            {
                MessageBox.Show("Lütfen e-posta ve parolanızı girin.", "Giriş",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                (string.IsNullOrWhiteSpace(txtEmail.Text) ? (Control)txtEmail : txtParola).Focus();
                return;
            }
            try
            {
                var k = kullaniciServisi.GirisYap(txtEmail.Text, txtParola.Text);
                Oturum.Baslat(k);
                txtParola.Clear();
                DialogResult = DialogResult.OK;
            }
            catch (UnauthorizedAccessException ex)
            {
                txtParola.Clear();
                MessageBox.Show(ex.Message, "Giriş başarısız",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParola.Focus();
            }
        }

        private void btnKayit_Click(object sender, EventArgs e)
        {
            using (var f = new KayitForm())
                f.ShowDialog(this);
        }

        private void GirisForm_Load(object sender, EventArgs e)
        {

        }
    }
}