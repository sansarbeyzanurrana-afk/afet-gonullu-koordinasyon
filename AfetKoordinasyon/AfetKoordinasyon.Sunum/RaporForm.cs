using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{
    // DURUM RAPORU EKRANI: özet kartlar + grafikler + tablolar + veri kalitesi
    public class RaporForm : Form
    {
        // Renkler tüm ekranlarda aynı anlamı taşır
        private static readonly Color RenkKarsilanan = Color.FromArgb(46, 139, 87);    // yeşil
        private static readonly Color RenkSurecte = Color.FromArgb(52, 120, 200);      // mavi
        private static readonly Color RenkBekleyen = Color.FromArgb(235, 140, 30);     // turuncu
        private static readonly Color RenkKritik = Color.FromArgb(200, 40, 40);        // kırmızı

        private readonly RaporServisi _raporServisi = new RaporServisi();
        private DurumRaporu _rapor;

        private readonly ComboBox cmbDonem = new ComboBox();
        private readonly Button btnYenile = new Button();
        private readonly Button btnDisaAktar = new Button();
        private readonly Button btnOzetKopyala = new Button();
        private readonly FlowLayoutPanel kartlar = new FlowLayoutPanel();
        private readonly Label lblOzet = new Label();
        private readonly TabControl sekmeler = new TabControl();

        private readonly Chart grafikKategori = new Chart();
        private readonly Chart grafikIlce = new Chart();
        private readonly Chart grafikAciliyet = new Chart();
        private readonly DataGridView dgvKategori = new DataGridView();
        private readonly DataGridView dgvIlce = new DataGridView();
        private readonly DataGridView dgvGonullu = new DataGridView();
        private readonly ListBox lstUyarilar = new ListBox();

        public RaporForm()
        {
            Text = "Durum Raporu";
            Size = new Size(1180, 800);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            // --- Üst çubuk ---
            var ust = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.LemonChiffon };
            ust.Controls.Add(new Label { Text = "Dönem:", AutoSize = true, Location = new Point(10, 13) });
            cmbDonem.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDonem.Items.AddRange(new object[] { "Tüm zamanlar", "Son 7 gün", "Son 30 gün" });
            cmbDonem.SelectedIndex = 0;
            cmbDonem.Location = new Point(65, 9); cmbDonem.Width = 130;
            btnYenile.Text = "Yenile"; btnYenile.Location = new Point(210, 8); btnYenile.Size = new Size(80, 27);
            btnDisaAktar.Text = "CSV olarak dışa aktar"; btnDisaAktar.Location = new Point(300, 8); btnDisaAktar.Size = new Size(160, 27);
            btnOzetKopyala.Text = "Özeti kopyala"; btnOzetKopyala.Location = new Point(470, 8); btnOzetKopyala.Size = new Size(110, 27);
            ust.Controls.AddRange(new Control[] { cmbDonem, btnYenile, btnDisaAktar, btnOzetKopyala });

            // --- Özet kartları ---
            kartlar.Dock = DockStyle.Top;
            kartlar.Height = 92;
            kartlar.Padding = new Padding(6);
            kartlar.BackColor = Color.WhiteSmoke;

            // --- Özet metni ---
            lblOzet.Dock = DockStyle.Top;
            lblOzet.Height = 46;
            lblOzet.Padding = new Padding(10, 4, 10, 4);
            lblOzet.BackColor = Color.White;

            // --- Sekmeler ---
            sekmeler.Dock = DockStyle.Fill;
            sekmeler.TabPages.Add(SekmeOlustur("Kategoriye göre", grafikKategori, dgvKategori));
            sekmeler.TabPages.Add(SekmeOlustur("Bölgeye (ilçe) göre", grafikIlce, dgvIlce));

            var aciliyetSekme = new TabPage("Aciliyet (açık talepler)");
            grafikAciliyet.Dock = DockStyle.Fill;
            aciliyetSekme.Controls.Add(grafikAciliyet);
            sekmeler.TabPages.Add(aciliyetSekme);

            var gonulluSekme = new TabPage("Gönüllüler");
            TabloAyarla(dgvGonullu); dgvGonullu.Dock = DockStyle.Fill;
            gonulluSekme.Controls.Add(dgvGonullu);
            sekmeler.TabPages.Add(gonulluSekme);

            var uyariSekme = new TabPage("Veri kalitesi");
            lstUyarilar.Dock = DockStyle.Fill;
            lstUyarilar.HorizontalScrollbar = true;
            uyariSekme.Controls.Add(lstUyarilar);
            sekmeler.TabPages.Add(uyariSekme);

            Controls.Add(sekmeler);
            Controls.Add(lblOzet);
            Controls.Add(kartlar);
            Controls.Add(ust);

            // --- Olaylar ---
            Load += (s, e) => Yukle();
            btnYenile.Click += (s, e) => Yukle();
            cmbDonem.SelectedIndexChanged += (s, e) => Yukle();
            btnOzetKopyala.Click += (s, e) =>
            {
                if (_rapor == null) return;
                Clipboard.SetText(_raporServisi.OzetMetni(_rapor));
                MessageBox.Show("Özet panoya kopyalandı.", "Durum Raporu");
            };
            btnDisaAktar.Click += (s, e) => DisaAktar();
        }

        // ------------------------------------------------------------------
        private void Yukle()
        {
            int? gun = cmbDonem.SelectedIndex == 1 ? 7 : cmbDonem.SelectedIndex == 2 ? 30 : (int?)null;
            try
            {
                _rapor = _raporServisi.Olustur(gun);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor oluşturulamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var r = _rapor;

            // Kartlar
            kartlar.Controls.Clear();
            kartlar.Controls.Add(Kart("Geçerli talep", r.Talep.ToString(), "iptal hariç (" + r.Iptal + " iptal)", Color.DimGray));
            kartlar.Controls.Add(Kart("Karşılanma oranı", r.KarsilanmaOrani, "tamamlanan / geçerli talep", RenkKarsilanan));
            kartlar.Controls.Add(Kart("Süreçte", r.Surecte.ToString(), "eşleşti + yolda", RenkSurecte));
            kartlar.Controls.Add(Kart("Bekleyen", r.Bekleyen.ToString(), "gönüllü bekliyor", RenkBekleyen));
            kartlar.Controls.Add(Kart("Kritik bekleyen", r.KritikBekleyen.ToString(), "aciliyet 4-5, bekliyor", RenkKritik));
            kartlar.Controls.Add(Kart("Ort. atanma süresi", RaporServisi.SureMetni(r.OrtAtanmaSaat, r.AtanmaOrnek), "bildirim → eşleşme", Color.DimGray));
            kartlar.Controls.Add(Kart("Ort. karşılanma süresi", RaporServisi.SureMetni(r.OrtKarsilanmaSaat, r.KarsilanmaOrnek), "bildirim → tamamlandı", Color.DimGray));
            kartlar.Controls.Add(Kart("Müsait gönüllü", r.GonulluMusait + " / " + r.GonulluToplam, "şu an atanabilir", Color.DimGray));

            lblOzet.Text = "Özet: " + _raporServisi.OzetMetni(r)
                         + "   (Oluşturma: " + r.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm") + ")";

            // Kategori ve ilçe: yığılmış sütun grafiği + tablo
            YiginGrafik(grafikKategori, r.Kategoriler, "Kategoriye göre talep durumu");
            YiginGrafik(grafikIlce, r.Ilceler, "Bölgeye göre talep durumu");
            dgvKategori.DataSource = TabloVerisi(r.Kategoriler);
            dgvIlce.DataSource = TabloVerisi(r.Ilceler);

            // Aciliyet dağılımı
            grafikAciliyet.Series.Clear(); grafikAciliyet.ChartAreas.Clear(); grafikAciliyet.Titles.Clear(); grafikAciliyet.Legends.Clear();
            var alan = AlanOlustur();
            grafikAciliyet.ChartAreas.Add(alan);
            grafikAciliyet.Titles.Add("Açık taleplerin aciliyet dağılımı (bekleyen + süreçte)");
            var seri = new Series("Talep sayısı") { ChartType = SeriesChartType.Column, IsValueShownAsLabel = true };
            foreach (var kv in r.AciliyetDagilimi)
            {
                int idx = seri.Points.AddXY(kv.Key, kv.Value);
                seri.Points[idx].Color = kv.Key.StartsWith("5") || kv.Key.StartsWith("4") ? RenkKritik
                                       : kv.Key.StartsWith("D") ? Color.Silver : RenkBekleyen;
            }
            grafikAciliyet.Series.Add(seri);

            // Gönüllüler
            dgvGonullu.DataSource = r.Gonulluler
                .Select(g => new { g.Gonullu, Bolge = g.Konum, g.Durum, TamamlananIs = g.TamamlananIs }).ToList();

            // Veri kalitesi
            lstUyarilar.Items.Clear();
            foreach (var u in r.VeriUyarilari) lstUyarilar.Items.Add("• " + u);
            sekmeler.TabPages[4].Text = r.VeriUyarilari.Count == 1 && r.VeriUyarilari[0].StartsWith("Belirgin")
                ? "Veri kalitesi" : "Veri kalitesi (" + r.VeriUyarilari.Count + ")";
        }

        private void YiginGrafik(Chart grafik, System.Collections.Generic.List<GrupSatiri> gruplar, string baslik)
        {
            grafik.Series.Clear(); grafik.ChartAreas.Clear(); grafik.Titles.Clear(); grafik.Legends.Clear();
            grafik.ChartAreas.Add(AlanOlustur());
            grafik.Titles.Add(baslik);
            grafik.Legends.Add(new Legend { Docking = Docking.Top, Alignment = StringAlignment.Center });

            var karsilanan = new Series("Karşılanan") { ChartType = SeriesChartType.StackedColumn, Color = RenkKarsilanan };
            var surecte = new Series("Süreçte") { ChartType = SeriesChartType.StackedColumn, Color = RenkSurecte };
            var bekleyen = new Series("Bekleyen") { ChartType = SeriesChartType.StackedColumn, Color = RenkBekleyen };
            foreach (var g in gruplar)
            {
                karsilanan.Points.AddXY(g.Ad, g.Karsilanan);
                surecte.Points.AddXY(g.Ad, g.Surecte);
                bekleyen.Points.AddXY(g.Ad, g.Bekleyen);
            }
            foreach (var s in new[] { karsilanan, surecte, bekleyen })
            {
                s.IsValueShownAsLabel = true;
                s.LabelForeColor = Color.White;
                // 0 olan dilimlerde etiket gösterme (görsel kirliliği önler)
                foreach (var p in s.Points) if (p.YValues[0] == 0) p.IsValueShownAsLabel = false;
                grafik.Series.Add(s);
            }
        }

        private static ChartArea AlanOlustur()
        {
            var a = new ChartArea();
            a.AxisY.Minimum = 0;                 // Y ekseni her zaman 0'dan başlar (yanıltıcı görseli önler)
            a.AxisY.Title = "Talep sayısı";
            a.AxisY.LabelStyle.Format = "0";
            a.AxisY.MajorGrid.LineColor = Color.Gainsboro;
            a.AxisX.MajorGrid.Enabled = false;
            a.AxisX.Interval = 1;
            a.AxisX.LabelStyle.Angle = -30;
            return a;
        }

        private static object TabloVerisi(System.Collections.Generic.List<GrupSatiri> gruplar)
        {
            return gruplar.Select(g => new
            {
                Ad = g.Ad,
                Talep = g.Talep,
                Karsilanan = g.Karsilanan,
                Surecte = g.Surecte,
                Bekleyen = g.Bekleyen,
                KritikBekleyen = g.KritikBekleyen,
                KarsilanmaOrani = g.KarsilanmaOrani
            }).ToList();
        }

        private TabPage SekmeOlustur(string baslik, Chart grafik, DataGridView tablo)
        {
            var sayfa = new TabPage(baslik);
            // Üstte grafik (%58), altta tablo (%42)
            var duzen = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            duzen.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            duzen.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            grafik.Dock = DockStyle.Fill;
            TabloAyarla(tablo); tablo.Dock = DockStyle.Fill;
            duzen.Controls.Add(grafik, 0, 0);
            duzen.Controls.Add(tablo, 0, 1);
            sayfa.Controls.Add(duzen);
            return sayfa;
        }

        private static Panel Kart(string baslik, string deger, string aciklama, Color renk)
        {
            var p = new Panel { Width = 131, Height = 74, BackColor = Color.White, Margin = new Padding(3) };
            p.Controls.Add(new Label { Text = baslik, Location = new Point(8, 4), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8F) });
            p.Controls.Add(new Label
            {
                Text = deger, Location = new Point(8, 22), AutoSize = true, ForeColor = renk,
                Font = new Font("Segoe UI", deger.Length > 9 ? 8.5F : 13F, FontStyle.Bold)
            });
            p.Controls.Add(new Label { Text = aciklama, Location = new Point(8, 52), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 7F) });
            p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 4, BackColor = renk });
            return p;
        }

        private static void TabloAyarla(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
        }

        // Raporu CSV dosyası olarak kaydeder (Excel'de açılabilir)
        private void DisaAktar()
        {
            if (_rapor == null) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Raporu dışa aktar";
                dlg.Filter = "CSV dosyası (*.csv)|*.csv";
                dlg.FileName = "DurumRaporu_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    // UTF-8 + BOM: Excel Türkçe karakterleri (ğ, ş, İ...) doğru gösterir
                    File.WriteAllText(dlg.FileName, _raporServisi.CsvOlustur(_rapor), new UTF8Encoding(true));
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Dosya kaydedilemedi (dosya Excel'de açık olabilir):\n" + ex.Message,
                        "Dışa aktarma", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Bu klasöre yazma izni yok:\n" + ex.Message,
                        "Dışa aktarma", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Rapor kaydedildi:\n" + dlg.FileName + "\n\nŞimdi açılsın mı?", "Dışa aktarma",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try { System.Diagnostics.Process.Start(dlg.FileName); }
                    catch (Exception) { /* CSV açacak program yoksa sessizce geç */ }
                }
            }
        }
    }
}
