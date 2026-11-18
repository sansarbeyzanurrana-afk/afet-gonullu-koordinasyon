using System;
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