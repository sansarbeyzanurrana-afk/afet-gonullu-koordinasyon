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
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void btnGiris_Click(object sender, EventArgs e)
        {
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
            }
        }

        private void btnKayit_Click(object sender, EventArgs e)
        {
            using (var f = new KayitForm())
                f.ShowDialog();
        }
    }
}