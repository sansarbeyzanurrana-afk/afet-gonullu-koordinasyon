using System;
using System.Drawing;
using System.Windows.Forms;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{
    public partial class KayitForm : Form
    {
        private readonly KullaniciServisi kullaniciServisi = new KullaniciServisi();

        // true: uygulama ilk kez kuruluyor, ilk koordinatör hesabı açılıyor
        private readonly bool _ilkKurulum;

        public KayitForm() : this(false) { }

        public KayitForm(bool ilkKurulum)
        {
            InitializeComponent();
            _ilkKurulum = ilkKurulum;

            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            if (ilkKurulum)
            {
                cmbRol.Items.Add("Koordinator");
                Text = "İlk Kurulum - Koordinatör Hesabı Oluşturun";
            }
            else
            {
                cmbRol.Items.Add("Afetzede");
                cmbRol.Items.Add("Gonullu");
                Text = "Yeni Kullanıcı Kaydı";
            }
            cmbRol.SelectedIndex = 0;
            GorunumuDuzenle(ilkKurulum);

            // KULLANILABİLİRLİK (Hafta 14)
            StartPosition = ilkKurulum ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
            AcceptButton = btnKaydet;                          // Enter = Kaydet
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };   // Esc = vazgeç
            var ipucu = new ToolTip();
            ipucu.SetToolTip(txtEmail, "Örnek: ad@ornek.com");
            ipucu.SetToolTip(txtParola, "En az 8 karakter");
            ipucu.SetToolTip(cmbRol, ilkKurulum ? "İlk hesap koordinatör olur" : "Afetzede: ihtiyaç bildirir. Gönüllü: yardım götürür.");
            txtAdSoyad.AccessibleName = "Ad soyad";
            txtEmail.AccessibleName = "E-posta";
            txtParola.AccessibleName = "Parola";
            txtParolaTekrar.AccessibleName = "Parola tekrar";
            cmbRol.AccessibleName = "Rol";
            Shown += (s, e) => txtAdSoyad.Focus();
        }
        // GÖRÜNÜM (Hafta 14): solda açıklama, sağda beyaz kayıt kartı
        private void GorunumuDuzenle(bool ilkKurulum)
        {
            SuspendLayout();
            Tema.Uygula(this, Properties.Resources.ArkaPlanGiris);
            Tema.Kontroller(Controls, koyuZemin: false);   // bu kontroller beyaz kartın içine girecek: koyu yazı
            ClientSize = new Size(980, 640);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var baslik = Tema.Baslik(ilkKurulum ? "İlk Kurulum" : "Aramıza\nkatılın", 28f, Color.White);
            baslik.Location = new Point(60, 170);
            var aciklama = new Label
            {
                Text = ilkKurulum
                    ? "Uygulama ilk kez çalışıyor.\nİhtiyaçları yönetecek koordinatör\nhesabını oluşturun."
                    : "Afetzede olarak ihtiyacınızı bildirin\nya da gönüllü olarak yardım edin.",
                AutoSize = true, BackColor = Color.Transparent, ForeColor = Tema.AcikYazi,
                Font = Tema.Yazi(12f), Location = new Point(64, ilkKurulum ? 250 : 300)
            };
            Controls.Add(baslik);
            Controls.Add(aciklama);

            label1.Text = "Ad Soyad";
            label2.Text = "E-posta";
            label3.Text = "Parola (en az 8 karakter)";
            label4.Text = "Parola tekrar";
            label5.Text = "Rol";
            Tema.ButonAna(btnKaydet);
            btnKaydet.Height = 42;
            btnKaydet.Text = ilkKurulum ? "Koordinatör hesabını oluştur" : "Kayıt ol";

            var kart = Tema.Kart(320);
            Tema.KartaEkle(kart, Tema.Baslik(ilkKurulum ? "Koordinatör hesabı" : "Yeni hesap", 18f, Tema.Metin));
            Tema.KartaEkle(kart, label1, 14); Tema.KartaEkle(kart, txtAdSoyad);
            Tema.KartaEkle(kart, label2, 8);  Tema.KartaEkle(kart, txtEmail);
            Tema.KartaEkle(kart, label3, 8);  Tema.KartaEkle(kart, txtParola);
            Tema.KartaEkle(kart, label4, 8);  Tema.KartaEkle(kart, txtParolaTekrar);
            Tema.KartaEkle(kart, label5, 8);  Tema.KartaEkle(kart, cmbRol);
            Tema.KartaEkle(kart, btnKaydet, 20);
            Controls.Add(kart);

            Load += (s, e) => kart.Location = new Point(ClientSize.Width - kart.Width - 70, (ClientSize.Height - kart.Height) / 2);
            ResumeLayout(true);
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (txtParola.Text != txtParolaTekrar.Text)
            {
                MessageBox.Show("Parolalar eşleşmiyor.", "Kayıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_ilkKurulum)
                    kullaniciServisi.IlkKoordinatoruOlustur(txtAdSoyad.Text, txtEmail.Text, txtParola.Text);
                else
                    kullaniciServisi.KayitOl(txtAdSoyad.Text, txtEmail.Text,
                        txtParola.Text, cmbRol.SelectedItem.ToString());
                MessageBox.Show("Kayıt başarılı. Şimdi bu hesapla giriş yapabilirsiniz.", "Kayıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;   // pencereyi kapatır
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Kayıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Kayıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}