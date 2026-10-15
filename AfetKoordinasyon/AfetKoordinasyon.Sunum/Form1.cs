using System;
using System.Linq;
using System.Windows.Forms;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Sunum
{
    public partial class Form1 : Form
    {
        private readonly IhtiyacServisi _ihtiyacServisi = new IhtiyacServisi();
        private readonly KonumServisi _konumServisi = new KonumServisi();

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            btnEkle.Click += btnEkle_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbKonum.DataSource = _konumServisi.Listele()
                .Select(k => new { k.KonumId, Ad = k.Il + " / " + k.Ilce + " / " + k.Mahalle })
                .ToList();
            cmbKonum.DisplayMember = "Ad";
            cmbKonum.ValueMember = "KonumId";

            ListeyiYenile();
        }

        private void ListeyiYenile()
        {
            dgvIhtiyaclar.DataSource = _ihtiyacServisi.Listele()
                .Select(i => new
                {
                    No = i.IhtiyacId,
                    i.Tur,
                    i.Aciklama,
                    i.Miktar,
                    Konum = i.Konum.Il + " / " + i.Konum.Ilce,
                    Durum = i.Durum.Ad,
                    i.Tarih
                })
                .ToList();
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            try
            {
                _ihtiyacServisi.Ekle(new Ihtiyac
                {
                    Tur = txtTur.Text.Trim(),
                    Aciklama = txtAciklama.Text.Trim(),
                    Miktar = (int)nudMiktar.Value,
                    KonumId = (int)cmbKonum.SelectedValue
                });

                MessageBox.Show("İhtiyaç kaydedildi.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTur.Clear();
                txtAciklama.Clear();
                nudMiktar.Value = 1;
                ListeyiYenile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}