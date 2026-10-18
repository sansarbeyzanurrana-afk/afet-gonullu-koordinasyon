using System;
using System.Windows.Forms;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{
    public partial class KayitForm : Form
    {
        private readonly KullaniciServisi kullaniciServisi = new KullaniciServisi();

        public KayitForm()
        {
            InitializeComponent();
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.Items.Add("Afetzede");
            cmbRol.Items.Add("Gonullu");
            cmbRol.SelectedIndex = 0;
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
                kullaniciServisi.KayitOl(txtAdSoyad.Text, txtEmail.Text,
                    txtParola.Text, cmbRol.SelectedItem.ToString());
                MessageBox.Show("Kayıt başarılı. Şimdi giriş yapabilirsiniz.", "Kayıt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
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