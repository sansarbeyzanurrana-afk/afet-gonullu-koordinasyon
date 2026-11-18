using System;
using System.Drawing;
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
        private readonly ToolTip _aiIpucu = new ToolTip { AutoPopDelay = 15000 };

        // BİLDİRİM (Hafta 12): zamanlayıcı ile kritik bekleyen talepleri kontrol eder
        private readonly BildirimServisi _bildirimServisi = new BildirimServisi();
        private readonly System.Windows.Forms.Timer _bildirimZamanlayici = new System.Windows.Forms.Timer();
        private readonly NotifyIcon _bildirimIkonu = new NotifyIcon();
        private readonly System.Collections.Generic.HashSet<int> _bildirilenler = new System.Collections.Generic.HashSet<int>();


        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            btnEkle.Click += btnEkle_Click;
            dgvIhtiyaclar.CellClick += dgvIhtiyaclar_CellClick;
            btnSil.Enabled = string.Equals(Oturum.AktifKullanici.Rol?.Trim(), "Koordinator", StringComparison.OrdinalIgnoreCase);
            var surum = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Text = "İhtiyaç Yönetimi - " + Oturum.AktifKullanici.AdSoyad
                   + " (" + Oturum.AktifKullanici.Rol + ")  ·  v" + surum.ToString(3);

            bool koordinator = string.Equals(Oturum.AktifKullanici.Rol?.Trim(), "Koordinator", StringComparison.OrdinalIgnoreCase);

            // Bildirimler sadece koordinatöre: atama yapabilen tek kişi o
            if (koordinator)
                BildirimleriBaslat();

            ArayuzuDuzenle(koordinator);
        }

        // KULLANILABİLİRLİK (Hafta 14): ipuçları, kısayol, rol'e göre görünen butonlar
        private void ArayuzuDuzenle(bool koordinator)
        {
            // GÖRÜNÜM: ortak tema + önemli butonlara renk
            Tema.Uygula(this, Properties.Resources.ArkaPlanAna);
            Tema.ButonAna(btnEkle);
            Tema.ButonTehlike(btnSil);
            Tema.ButonVurgu(btnAiOner);
            btnAiOner.Font = Tema.Kalin(9f);
            foreach (var b in new[] { btnRapor, btnDurumTakip, btnEslestirme, btnHarita })
                b.Font = Tema.Kalin(11f);
            lblAiSonuc.BackColor = Color.FromArgb(240, 248, 255);
            lblAiSonuc.ForeColor = Tema.Metin;
            lblAiSonuc.Font = Tema.Yazi(8.5f);
            lblAiSonuc.Padding = new Padding(4, 2, 4, 2);
            lblAiSonuc.AutoEllipsis = true;      // sığmayan metin "..." ile biter, tamamı ipucunda görünür

            // Koordinatöre özel ekranlar diğer rollerde gizlenir
            // (önceden görünüyordu ama işlem yapınca yetki hatası veriyordu)
            btnEslestirme.Visible = koordinator;
            btnRapor.Visible = koordinator;

            var ipucu = new ToolTip { AutoPopDelay = 8000 };
            ipucu.SetToolTip(btnAiOner, "Açıklamaya göre kategori ve aciliyet önerir. Yapay zekâ çalışmazsa kural tabanlı yedek kullanılır.");
            ipucu.SetToolTip(btnEkle, "Yeni ihtiyacı kaydeder");
            ipucu.SetToolTip(btnGuncelle, "Listeden seçili ihtiyacı günceller");
            ipucu.SetToolTip(btnSil, koordinator ? "Seçili ihtiyacı siler" : "Sadece koordinatör silebilir");
            ipucu.SetToolTip(btnRapor, "Durum raporu, grafikler ve CSV dışa aktarma");
            ipucu.SetToolTip(btnDurumTakip, "Durumları güncelle, gönüllü ata, geçmişi gör");
            ipucu.SetToolTip(btnEslestirme, "Bekleyen ihtiyaçlara müsait gönüllü ata");
            ipucu.SetToolTip(btnHarita, "Bölgelere göre ihtiyaç ve gönüllü haritası");
            ipucu.SetToolTip(dgvIhtiyaclar, "F5: listeyi yenile");

            // Ekran okuyucular için alan adları
            txtTur.AccessibleName = "Tür";
            txtAciklama.AccessibleName = "Açıklama";
            nudMiktar.AccessibleName = "Miktar";
            cmbKonum.AccessibleName = "Konum";
            cmbKategori.AccessibleName = "Kategori";
            dgvIhtiyaclar.AccessibleName = "İhtiyaç listesi";

            // F5: listeyi yenile
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5) { ListeyiYenile(); e.Handled = true; }
            };
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

        // ------------------------------------------------------------------
        // BİLDİRİM
        // ------------------------------------------------------------------
        private void BildirimleriBaslat()
        {
            _bildirimIkonu.Icon = System.Drawing.SystemIcons.Warning;
            _bildirimIkonu.Text = "Afet Koordinasyon - bildirimler";
            _bildirimIkonu.Visible = true;
            _bildirimIkonu.BalloonTipClicked += (s, e) => btnDurumTakip_Click(s, e);   // tıklayınca Durum Takibi açılır
            _bildirimIkonu.DoubleClick += (s, e) => btnDurumTakip_Click(s, e);

            _bildirimZamanlayici.Interval = 5 * 60 * 1000;      // 5 dakikada bir kontrol
            _bildirimZamanlayici.Tick += (s, e) => KritikleriKontrolEt();
            _bildirimZamanlayici.Start();

            Shown += (s, e) => KritikleriKontrolEt();            // açılışta da bir kez kontrol et

            // Form kapanınca bildirim simgesi tepside asılı kalmasın
            FormClosed += (s, e) =>
            {
                _bildirimZamanlayici.Stop();
                _bildirimIkonu.Visible = false;
                _bildirimIkonu.Dispose();
            };
        }

        private void KritikleriKontrolEt()
        {
            System.Collections.Generic.List<KritikBildirim> kritikler;
            try
            {
                kritikler = _bildirimServisi.UzunSuredirBekleyenKritikler();
            }
            catch (Exception)
            {
                return;   // bildirim kontrolü başarısız olursa programı rahatsız etme, sonraki turda tekrar dener
            }

            // Sadece DAHA ÖNCE bildirilmemiş talepler için bildirim gönder (aynı uyarıyı tekrar tekrar gösterme)
            var yeniler = kritikler.Where(k => !_bildirilenler.Contains(k.IhtiyacId)).ToList();
            // Artık kritik olmayanlar (atanmış/tamamlanmış) listeden çıkar; tekrar beklemeye düşerse yeniden bildirilir
            _bildirilenler.IntersectWith(kritikler.Select(k => k.IhtiyacId));
            if (yeniler.Count == 0) return;
            foreach (var k in yeniler) _bildirilenler.Add(k.IhtiyacId);

            string metin = yeniler.Count == 1
                ? yeniler[0].Ozet + " " + BildirimServisi.SureMetni(yeniler[0].BeklemeSaati) + " gönüllü bekliyor."
                : yeniler.Count + " kritik talep 1 saatten uzun süredir gönüllü bekliyor. En uzun: "
                  + yeniler[0].Ozet + " (" + BildirimServisi.SureMetni(yeniler[0].BeklemeSaati) + ").";

            _bildirimIkonu.ShowBalloonTip(10000, "Kritik ihtiyaç bekliyor", metin + " Açmak için tıklayın.", ToolTipIcon.Warning);
        }

        private void btnRapor_Click(object sender, EventArgs e)
        {
            using (var f = new RaporForm())
                f.ShowDialog();
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

                // Önemli bilgi ilk satırda; kaynak ve gerekçe altta. Tam metin fareyle üzerine gelince görünür.
                lblAiSonuc.Text = "Öneri: " + oneri.KategoriAd + "  ·  Aciliyet " + oneri.Aciliyet + "/5"
                                + "  (" + oneri.Kaynak + ")  - kontrol edip Ekle'ye basın\n"
                                + oneri.Gerekce;
                _aiIpucu.SetToolTip(lblAiSonuc, lblAiSonuc.Text);

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
