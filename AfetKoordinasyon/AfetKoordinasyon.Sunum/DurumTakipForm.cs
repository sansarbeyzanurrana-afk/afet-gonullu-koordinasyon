using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{
    // DURUM TAKİBİ VE KOORDİNASYON EKRANI
    // Üstte filtreli ihtiyaç listesi, altta seçili ihtiyacın geçmişi ve yapılabilecek işlemler.
    // Butonlar durum makinesine göre açılıp kapanır: sadece izin verilen işlemler tıklanabilir.
    public class DurumTakipForm : Form
    {
        private readonly DurumServisi _durumServisi = new DurumServisi();
        private readonly EslesmeServisi _eslesmeServisi = new EslesmeServisi();

        private readonly ComboBox cmbDurumFiltre = new ComboBox();
        private readonly ComboBox cmbAciliyetFiltre = new ComboBox();
        private readonly Button btnYenile = new Button();
        private readonly DataGridView dgvTakip = new DataGridView();
        private readonly DataGridView dgvGecmis = new DataGridView();
        private readonly Label lblSecili = new Label();
        private readonly Label lblIzinli = new Label();
        private readonly ComboBox cmbGonullu = new ComboBox();
        private readonly Button btnAta = new Button();
        private readonly Button btnYolda = new Button();
        private readonly Button btnTamamla = new Button();
        private readonly Button btnAtamaKaldir = new Button();
        private readonly Button btnIptal = new Button();
        private readonly TextBox txtNot = new TextBox();
        private readonly Button btnNotEkle = new Button();

        private TakipSatiri _secili;

        public DurumTakipForm()
        {
            Text = "Durum Takibi ve Koordinasyon";
            Size = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            // --- Üst: filtreler ---
            var ust = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.LemonChiffon };
            ust.Controls.Add(new Label { Text = "Durum:", AutoSize = true, Location = new Point(10, 13) });
            cmbDurumFiltre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDurumFiltre.Items.AddRange(new object[] { "Tümü", DurumAdlari.Bekliyor, DurumAdlari.Eslesti,
                                                         DurumAdlari.Yolda, DurumAdlari.Tamamlandi, DurumAdlari.Iptal });
            cmbDurumFiltre.SelectedIndex = 0;
            cmbDurumFiltre.Location = new Point(65, 9);
            cmbDurumFiltre.Width = 130;

            ust.Controls.Add(new Label { Text = "En az aciliyet:", AutoSize = true, Location = new Point(215, 13) });
            cmbAciliyetFiltre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAciliyetFiltre.Items.AddRange(new object[] { "Tümü", "3+", "4+", "5" });
            cmbAciliyetFiltre.SelectedIndex = 0;
            cmbAciliyetFiltre.Location = new Point(315, 9);
            cmbAciliyetFiltre.Width = 80;

            btnYenile.Text = "Yenile";
            btnYenile.Location = new Point(410, 8);
            btnYenile.Size = new Size(80, 27);

            ust.Controls.Add(cmbDurumFiltre);
            ust.Controls.Add(cmbAciliyetFiltre);
            ust.Controls.Add(btnYenile);

            // --- Orta: takip listesi ---
            TabloAyarla(dgvTakip);
            dgvTakip.Dock = DockStyle.Fill;

            // --- Alt: geçmiş (sol) + işlemler (sağ) ---
            var alt = new Panel { Dock = DockStyle.Bottom, Height = 300 };

            TabloAyarla(dgvGecmis);
            dgvGecmis.Dock = DockStyle.Fill;
            var gecmisKutu = new GroupBox { Text = "Durum geçmişi (en yeni üstte)", Dock = DockStyle.Fill };
            gecmisKutu.Controls.Add(dgvGecmis);

            var islemler = new GroupBox { Text = "İşlemler", Dock = DockStyle.Right, Width = 380 };
            lblSecili.Location = new Point(10, 22); lblSecili.Size = new Size(360, 20);
            lblSecili.Font = new Font(Font, FontStyle.Bold);
            lblIzinli.Location = new Point(10, 44); lblIzinli.Size = new Size(360, 34);
            lblIzinli.ForeColor = Color.DimGray;

            cmbGonullu.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGonullu.Location = new Point(10, 82); cmbGonullu.Width = 230;
            btnAta.Text = "Gönüllü Ata"; btnAta.Location = new Point(250, 80); btnAta.Size = new Size(120, 27);

            btnYolda.Text = "Yola Çıktı";        btnYolda.Location = new Point(10, 115);       btnYolda.Size = new Size(115, 30);
            btnTamamla.Text = "Tamamlandı";      btnTamamla.Location = new Point(132, 115);    btnTamamla.Size = new Size(115, 30);
            btnAtamaKaldir.Text = "Atamayı Kaldır"; btnAtamaKaldir.Location = new Point(255, 115); btnAtamaKaldir.Size = new Size(115, 30);
            btnIptal.Text = "İptal Et";          btnIptal.Location = new Point(10, 150);       btnIptal.Size = new Size(115, 30);
            btnIptal.ForeColor = Color.DarkRed;

            islemler.Controls.Add(new Label
            {
                Text = "Not (İptal ve Atamayı Kaldır için zorunlu):",
                Location = new Point(10, 188), AutoSize = true
            });
            txtNot.Multiline = true; txtNot.MaxLength = 500;
            txtNot.Location = new Point(10, 208); txtNot.Size = new Size(360, 50);
            btnNotEkle.Text = "Sadece Not Ekle"; btnNotEkle.Location = new Point(250, 262); btnNotEkle.Size = new Size(120, 27);

            islemler.Controls.AddRange(new Control[] { lblSecili, lblIzinli, cmbGonullu, btnAta, btnYolda, btnTamamla,
                                                       btnAtamaKaldir, btnIptal, txtNot, btnNotEkle });
            alt.Controls.Add(gecmisKutu);
            alt.Controls.Add(islemler);

            Controls.Add(dgvTakip);
            Controls.Add(alt);
            Controls.Add(ust);

            // --- Olaylar ---
            Load += (s, e) => ListeyiYenile();
            btnYenile.Click += (s, e) => ListeyiYenile();
            cmbDurumFiltre.SelectedIndexChanged += (s, e) => ListeyiYenile();
            cmbAciliyetFiltre.SelectedIndexChanged += (s, e) => ListeyiYenile();
            dgvTakip.SelectionChanged += (s, e) => SeciliDegisti();

            btnAta.Click += (s, e) => Islem(() =>
            {
                var g = cmbGonullu.SelectedItem as GonulluSecenek;
                if (g == null) throw new ArgumentException("Önce listeden bir gönüllü seçin.");
                _durumServisi.Ata(_secili.IhtiyacId, g.Id, _secili.SatirSurumu, BosIseNull(txtNot.Text));
            }, "Gönüllü atandı.");
            btnYolda.Click += (s, e) => Islem(() =>
                _durumServisi.YolaCikti(_secili.IhtiyacId, _secili.SatirSurumu, BosIseNull(txtNot.Text)), "Durum: Yolda.");
            btnTamamla.Click += (s, e) => Islem(() =>
                _durumServisi.Tamamla(_secili.IhtiyacId, _secili.SatirSurumu, BosIseNull(txtNot.Text)),
                "İhtiyaç tamamlandı, gönüllü tekrar müsait.");
            btnAtamaKaldir.Click += (s, e) => Islem(() =>
                _durumServisi.AtamayiKaldir(_secili.IhtiyacId, txtNot.Text, _secili.SatirSurumu),
                "Atama kaldırıldı, ihtiyaç yeniden 'Bekliyor'.");
            btnIptal.Click += (s, e) =>
            {
                if (MessageBox.Show("Bu ihtiyaç iptal edilsin mi? Bu işlem geri alınamaz.", "Onay",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                Islem(() => _durumServisi.IptalEt(_secili.IhtiyacId, txtNot.Text, _secili.SatirSurumu), "İhtiyaç iptal edildi.");
            };
            btnNotEkle.Click += (s, e) => Islem(() => _durumServisi.NotEkle(_secili.IhtiyacId, txtNot.Text), "Not eklendi.");

            ButonlariAyarla(null);
        }

        // ------------------------------------------------------------------
        private void ListeyiYenile()
        {
            var seciliId = _secili?.IhtiyacId;
            string durum = cmbDurumFiltre.SelectedIndex <= 0 ? null : (string)cmbDurumFiltre.SelectedItem;
            int? enAz = cmbAciliyetFiltre.SelectedIndex <= 0 ? (int?)null : new[] { 0, 3, 4, 5 }[cmbAciliyetFiltre.SelectedIndex];

            var liste = _durumServisi.TakipListesi(durum, enAz);
            dgvTakip.Tag = liste;   // DataSource değişince seçim olayı tetiklenir; liste önceden hazır olsun
            dgvTakip.DataSource = liste.Select(t => new
            {
                No = t.IhtiyacId,
                Aciliyet = t.Aciliyet.HasValue ? t.Aciliyet + "/5" : "-",
                t.Durum,
                t.Tur,
                t.Aciklama,
                t.Kategori,
                t.Konum,
                Gonullu = t.Gonullu ?? "-",
                SonDegisiklik = t.SonDegisiklik
            }).ToList();

            // Önceki seçimi koru
            if (seciliId.HasValue)
                foreach (DataGridViewRow r in dgvTakip.Rows)
                    if ((int)r.Cells["No"].Value == seciliId.Value) { r.Selected = true; dgvTakip.CurrentCell = r.Cells[0]; break; }
            SeciliDegisti();
        }

        private void SeciliDegisti()
        {
            var liste = dgvTakip.Tag as System.Collections.Generic.List<TakipSatiri>;
            if (liste == null || dgvTakip.CurrentRow == null || dgvTakip.CurrentRow.Index >= liste.Count)
            {
                _secili = null;
            }
            else
            {
                int id = (int)dgvTakip.CurrentRow.Cells["No"].Value;
                _secili = liste.FirstOrDefault(t => t.IhtiyacId == id);
            }

            if (_secili == null)
            {
                lblSecili.Text = "Listeden bir ihtiyaç seçin.";
                lblIzinli.Text = "";
                dgvGecmis.DataSource = null;
                ButonlariAyarla(null);
                return;
            }

            lblSecili.Text = "#" + _secili.IhtiyacId + " - " + _secili.Tur + "  [" + _secili.Durum + "]";
            var sonraki = DurumServisi.SonrakiDurumlar(_secili.Durum);
            lblIzinli.Text = sonraki.Length == 0
                ? "Bu son durum; başka geçiş yapılamaz."
                : "İzinli geçişler: " + _secili.Durum + " → " + string.Join(", ", sonraki);

            dgvGecmis.DataSource = _durumServisi.GecmisiGetir(_secili.IhtiyacId)
                .Select(g => new { g.Tarih, g.Degisiklik, g.Kullanici, g.Not }).ToList();

            ButonlariAyarla(_secili.Durum);
        }

        // Durum makinesine göre sadece izin verilen butonlar aktif olur
        private void ButonlariAyarla(string durum)
        {
            var sonraki = DurumServisi.SonrakiDurumlar(durum);
            btnAta.Enabled = cmbGonullu.Enabled = sonraki.Contains(DurumAdlari.Eslesti);
            btnYolda.Enabled = sonraki.Contains(DurumAdlari.Yolda);
            btnTamamla.Enabled = sonraki.Contains(DurumAdlari.Tamamlandi);
            btnAtamaKaldir.Enabled = sonraki.Contains(DurumAdlari.Bekliyor);
            btnIptal.Enabled = sonraki.Contains(DurumAdlari.Iptal);
            btnNotEkle.Enabled = durum != null;

            cmbGonullu.Items.Clear();
            if (btnAta.Enabled && _secili != null)
            {
                foreach (var g in _eslesmeServisi.UygunGonulluleriGetir(_secili.IhtiyacId))
                    cmbGonullu.Items.Add(new GonulluSecenek { Id = g.GonulluId, Ad = g.AdSoyad + " (" + g.Beceri + ")" });
                if (cmbGonullu.Items.Count > 0) cmbGonullu.SelectedIndex = 0;
                else { cmbGonullu.Items.Add("Bu konumda müsait gönüllü yok"); cmbGonullu.SelectedIndex = 0; btnAta.Enabled = false; }
            }
        }

        // Tüm işlemler için ortak hata yönetimi
        private void Islem(Action islem, string basariMesaji)
        {
            if (_secili == null) { MessageBox.Show("Önce listeden bir ihtiyaç seçin."); return; }
            try
            {
                islem();
                txtNot.Clear();
                MessageBox.Show(basariMesaji, "Durum Takibi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Yetki", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "İşlem yapılamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            ListeyiYenile();   // başarılı da olsa hata da olsa en güncel durumu göster
        }

        private static string BosIseNull(string metin)
        {
            return string.IsNullOrWhiteSpace(metin) ? null : metin;
        }

        private static void TabloAyarla(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.BackgroundColor = Color.White;
        }

        private class GonulluSecenek
        {
            public int Id { get; set; }
            public string Ad { get; set; }
            public override string ToString() { return Ad; }
        }
    }
}
