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
        private readonly KategoriServisi _kategoriServisi = new KategoriServisi();


        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            btnEkle.Click += btnEkle_Click;
            dgvIhtiyaclar.CellClick += dgvIhtiyaclar_CellClick;
            btnSil.Enabled = Oturum.AktifKullanici.Rol == "Koordinator";
            Text = "İhtiyaç Yönetimi - " + Oturum.AktifKullanici.AdSoyad
                   + " (" + Oturum.AktifKullanici.Rol + ")";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbKonum.DataSource = _konumServisi.Listele()
                .Select(k => new { k.KonumId, Ad = k.Il + " / " + k.Ilce + " / " + k.Mahalle })
                .ToList();
            cmbKonum.DisplayMember = "Ad";
            cmbKonum.ValueMember = "KonumId";
            cmbKategori.DataSource = _kategoriServisi.Listele();
            cmbKategori.DisplayMember = "Ad";
            cmbKategori.ValueMember = "Id";


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
                    Kategori = i.Kategori.Ad,
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
                    KonumId = (int)cmbKonum.SelectedValue,
                    KategoriId = (int)cmbKategori.SelectedValue
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
        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvIhtiyaclar.CurrentRow == null)
            {
                MessageBox.Show("Önce listeden bir ihtiyaç seçin.");
                return;
            }
            try
            {
                int id = (int)dgvIhtiyaclar.CurrentRow.Cells["No"].Value;
                _ihtiyacServisi.Guncelle(new Ihtiyac
                {
                    IhtiyacId = id,
                    Tur = txtTur.Text.Trim(),
                    Aciklama = txtAciklama.Text.Trim(),
                    Miktar = (int)nudMiktar.Value,
                    KonumId = (int)cmbKonum.SelectedValue,
                    KategoriId = (int)cmbKategori.SelectedValue
                });
                MessageBox.Show("İhtiyaç güncellendi.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ListeyiYenile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dgvIhtiyaclar.CurrentRow == null)
            {
                MessageBox.Show("Önce listeden bir ihtiyaç seçin.");
                return;
            }
            var cevap = MessageBox.Show("Seçili ihtiyaç silinsin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cevap != DialogResult.Yes) return;

            try
            {
                int id = (int)dgvIhtiyaclar.CurrentRow.Cells["No"].Value;
                _ihtiyacServisi.Sil(id);
                MessageBox.Show("İhtiyaç silindi.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ListeyiYenile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvIhtiyaclar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var satir = dgvIhtiyaclar.Rows[e.RowIndex];
            txtTur.Text = satir.Cells["Tur"].Value?.ToString();
            txtAciklama.Text = satir.Cells["Aciklama"].Value?.ToString();
            nudMiktar.Value = Convert.ToDecimal(satir.Cells["Miktar"].Value);
            cmbKonum.SelectedIndex = cmbKonum.FindString(satir.Cells["Konum"].Value?.ToString());
            cmbKategori.SelectedIndex = cmbKategori.FindStringExact(satir.Cells["Kategori"].Value?.ToString());
        }

        private void btnCikis_Click(object sender, EventArgs e)
        {
            Oturum.Cikis();
            Close();
        }

        private void txtAciklama_TextChanged(object sender, EventArgs e)
        {

        }
    }
}