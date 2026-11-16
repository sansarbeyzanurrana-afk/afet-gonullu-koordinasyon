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
        private readonly IhtiyacOnceliklendirmeServisi _onceliklendirmeServisi = new IhtiyacOnceliklendirmeServisi();

        // Son alınan AI önerisi (açıklama değişirse geçersiz olur)
        private OncelikSonucu _sonOneri;


        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            btnEkle.Click += btnEkle_Click;
            dgvIhtiyaclar.CellClick += dgvIhtiyaclar_CellClick;
            btnSil.Enabled = string.Equals(Oturum.AktifKullanici.Rol?.Trim(), "Koordinator", StringComparison.OrdinalIgnoreCase);
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
                    Aciliyet = i.AciliyetSkoru.HasValue ? i.AciliyetSkoru + "/5" : "-",
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
                    KategoriId = (int)cmbKategori.SelectedValue,
                    AciliyetSkoru = _sonOneri?.Aciliyet,
                    AiGerekce = _sonOneri == null ? null : (_sonOneri.Kaynak + ": " + _sonOneri.Gerekce)
                });

                MessageBox.Show("İhtiyaç kaydedildi.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTur.Clear();
                txtAciklama.Clear();
                nudMiktar.Value = 1;
                _sonOneri = null;
                lblAiSonuc.Text = "Açıklamayı yazıp \"AI ile Öner\"e basın.";
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
                    KategoriId = (int)cmbKategori.SelectedValue,
                    AciliyetSkoru = _sonOneri?.Aciliyet,
                    AiGerekce = _sonOneri == null ? null : (_sonOneri.Kaynak + ": " + _sonOneri.Gerekce)
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
            // Metin değişti: eski öneri artık bu metne ait değil
            if (_sonOneri != null)
            {
                _sonOneri = null;
                lblAiSonuc.Text = "Açıklama değişti; yeni öneri için \"AI ile Öner\"e basın.";
            }
        }

       
        private void btnEslestirme_Click(object sender, EventArgs e)
        {
            new EslestirmeForm().ShowDialog();
            ListeyiYenile();
        }

        private void btnDurumTakip_Click(object sender, EventArgs e)
        {
            using (var f = new DurumTakipForm())
                f.ShowDialog();
            ListeyiYenile();   // durumlar değişmiş olabilir
        }

        private void btnHarita_Click(object sender, EventArgs e)
        {
            using (var f = new HaritaForm())
                f.ShowDialog();
        }

        // Açıklamayı yapay zekâya gönderir; kategori ve aciliyet ÖNERİSİ alır.
        // Öneri otomatik kaydedilmez: kullanıcı görür, gerekirse kategoriyi değiştirir, sonra Ekle'ye basar.
        private async void btnAiOner_Click(object sender, EventArgs e)
        {
            btnAiOner.Enabled = false;
            lblAiSonuc.Text = "Yapay zekâ değerlendiriyor...";
            try
            {
                var oneri = await _onceliklendirmeServisi.DegerlendirAsync(txtAciklama.Text);
                _sonOneri = oneri;

                cmbKategori.SelectedValue = oneri.KategoriId;
                if (string.IsNullOrWhiteSpace(txtTur.Text))
                    txtTur.Text = oneri.KategoriAd;

                lblAiSonuc.Text = "ÖNERİ (" + oneri.Kaynak + ") - lütfen kontrol edin:\n"
                                + "Kategori: " + oneri.KategoriAd + "  |  Aciliyet: " + oneri.Aciliyet + "/5\n"
                                + oneri.Gerekce;

                if (oneri.Uyari != null)
                    MessageBox.Show(oneri.Uyari, "Yapay zekâ kullanılamadı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                lblAiSonuc.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblAiSonuc.Text = "Öneri alınamadı.";
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAiOner.Enabled = true;
            }
        }
    }
    }
