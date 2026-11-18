using System;
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

            // KULLANILABİLİRLİK (Hafta 14)
            Text = "Giriş - Afet Sonrası İhtiyaç ve Gönüllü Koordinasyon";
            StartPosition = FormStartPosition.CenterScreen;
            AcceptButton = btnGiris;                         // Enter ile giriş yapılır
            txtEmail.AccessibleName = "E-posta";             // ekran okuyucular için alan adları
            txtParola.AccessibleName = "Parola";
            btnKayit.AccessibleDescription = "Yeni kullanıcı kaydı açar";
            Shown += (s, e) => txtEmail.Focus();             // açılınca imleç e-postada
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