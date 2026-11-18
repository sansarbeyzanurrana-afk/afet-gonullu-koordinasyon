using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AfetKoordinasyon.Is;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace AfetKoordinasyon.Sunum
{
    // HARİTA GÖRÜNÜMÜ: bölgelerin durum özeti harita üzerinde ve yandaki tabloda.
    // İşaret rengi o bölgedeki EN ACİL duruma göre seçilir.
    public class HaritaForm : Form
    {
        private readonly HaritaServisi _haritaServisi = new HaritaServisi();
        private readonly KoordinatServisi _koordinatServisi = new KoordinatServisi();

        private readonly GMapControl gMap = new GMapControl();
        private readonly GMapOverlay acikKatman = new GMapOverlay("acik");
        private readonly GMapOverlay karsilananKatman = new GMapOverlay("karsilanan");
        private readonly GMapOverlay gonulluKatman = new GMapOverlay("gonullu");

        private readonly Button btnKoordinat = new Button();
        private readonly Button btnYenidenAl = new Button();
        private readonly CheckBox chkAcik = new CheckBox();
        private readonly CheckBox chkKarsilanan = new CheckBox();
        private readonly CheckBox chkGonullu = new CheckBox();
        private readonly Label lblDurum = new Label();
        private readonly Label lblKaynak = new Label();
        private readonly DataGridView dgvBolgeler = new DataGridView();

        private List<BolgeOzeti> _bolgeler = new List<BolgeOzeti>();
        private static readonly PointLatLng Baslangic = new PointLatLng(38.885, 40.498);   // Bingöl

        public HaritaForm()
        {
            Text = "Harita Görünümü - Bölge Durum Özeti";
            Size = new Size(1200, 760);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);

            // --- Üst panel: katman filtreleri ---
            var ust = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.LemonChiffon };
            btnKoordinat.Text = "Koordinatları Güncelle";
            btnKoordinat.Location = new Point(10, 8); btnKoordinat.Size = new Size(170, 28);
            btnYenidenAl.Text = "Koordinatları yeniden al";
            btnYenidenAl.Location = new Point(186, 8); btnYenidenAl.Size = new Size(170, 28);
            chkAcik.Text = "Açık talepler"; chkAcik.Checked = true; chkAcik.AutoSize = true; chkAcik.Location = new Point(370, 13);
            chkKarsilanan.Text = "Karşılananlar"; chkKarsilanan.Checked = false; chkKarsilanan.AutoSize = true; chkKarsilanan.Location = new Point(480, 13);
            chkGonullu.Text = "Gönüllüler"; chkGonullu.Checked = true; chkGonullu.AutoSize = true; chkGonullu.Location = new Point(595, 13);
            lblDurum.AutoSize = true; lblDurum.Location = new Point(690, 14);
            ust.Controls.AddRange(new Control[] { btnKoordinat, btnYenidenAl, chkAcik, chkKarsilanan, chkGonullu, lblDurum });

            // --- Sağ panel: lejant + bölge özeti tablosu ---
            var sag = new Panel { Dock = DockStyle.Right, Width = 380, Padding = new Padding(6) };
            var lejant = new GroupBox { Text = "Renklerin anlamı (bölgedeki en acil durum)", Dock = DockStyle.Top, Height = 150 };
            LejantSatiri(lejant, 0, Color.FromArgb(220, 50, 50), "Kırmızı: kritik bekleyen var (aciliyet 4-5)");
            LejantSatiri(lejant, 1, Color.FromArgb(240, 140, 30), "Turuncu: bekleyen talep var");
            LejantSatiri(lejant, 2, Color.FromArgb(60, 120, 220), "Mavi: talepler süreçte (eşleşti / yolda)");
            LejantSatiri(lejant, 3, Color.Gray, "Gri (küçük): sadece karşılanmış talepler");
            LejantSatiri(lejant, 4, Color.FromArgb(40, 170, 70), "Yeşil (küçük): gönüllüler (biraz doğuya kaydırılmış)");

            var tabloKutu = new GroupBox { Text = "Bölge özeti (satıra tıklayınca haritada gösterir)", Dock = DockStyle.Fill };
            dgvBolgeler.Dock = DockStyle.Fill;
            dgvBolgeler.ReadOnly = true; dgvBolgeler.AllowUserToAddRows = false; dgvBolgeler.RowHeadersVisible = false;
            dgvBolgeler.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvBolgeler.MultiSelect = false;
            dgvBolgeler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvBolgeler.BackgroundColor = Color.White;
            tabloKutu.Controls.Add(dgvBolgeler);
            sag.Controls.Add(tabloKutu);
            sag.Controls.Add(lejant);

            // --- Alt bilgi: veri kaynakları ---
            lblKaynak.Dock = DockStyle.Bottom; lblKaynak.Height = 22; lblKaynak.TextAlign = ContentAlignment.MiddleRight;
            lblKaynak.Text = "Harita: © OpenStreetMap katkıcıları  |  Search by LocationIQ.com  ";

            // --- Harita ---
            gMap.Dock = DockStyle.Fill;
            gMap.MapProvider = GMapProviders.OpenStreetMap;
            gMap.Position = Baslangic;
            gMap.MinZoom = 3; gMap.MaxZoom = 18; gMap.Zoom = 9;
            gMap.DragButton = MouseButtons.Left;
            gMap.ShowCenter = false;
            gMap.Overlays.Add(karsilananKatman);
            gMap.Overlays.Add(acikKatman);
            gMap.Overlays.Add(gonulluKatman);
            gMap.OnMarkerClick += (item, e) =>
            {
                if (item.Tag is string detay)
                    MessageBox.Show(detay, "Bölge ayrıntısı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Controls.Add(gMap);
            Controls.Add(sag);
            Controls.Add(ust);
            Controls.Add(lblKaynak);
            Tema.UygulaAcik(this);   // ortak tema (Hafta 14)

            // --- Olaylar ---
            Load += HaritaForm_Load;
            btnKoordinat.Click += async (s, e) => await KoordinatlariGuncelleAsync(false);
            btnYenidenAl.Click += async (s, e) =>
            {
                if (MessageBox.Show("Tüm konumların koordinatı silinip LocationIQ'dan yeniden alınacak. Devam edilsin mi?",
                        "Koordinatları yeniden al", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    await KoordinatlariGuncelleAsync(true);
            };
            chkAcik.CheckedChanged += (s, e) => acikKatman.IsVisibile = chkAcik.Checked;
            chkKarsilanan.CheckedChanged += (s, e) => karsilananKatman.IsVisibile = chkKarsilanan.Checked;
            chkGonullu.CheckedChanged += (s, e) => gonulluKatman.IsVisibile = chkGonullu.Checked;
            dgvBolgeler.CellClick += (s, e) => BolgeyeGit(e.RowIndex);
        }

        private async void HaritaForm_Load(object sender, EventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            GMapProvider.UserAgent = "AfetKoordinasyon/1.0 (Bingol Universitesi ogrenci projesi)";
            karsilananKatman.IsVisibile = chkKarsilanan.Checked;

            Ciz();
            if (_haritaServisi.KoordinatiEksikKonumSayisi() > 0)
                await KoordinatlariGuncelleAsync(false);
        }

        private async System.Threading.Tasks.Task KoordinatlariGuncelleAsync(bool hepsiniYenidenAl)
        {
            btnKoordinat.Enabled = btnYenidenAl.Enabled = false;
            lblDurum.Text = "Koordinatlar LocationIQ'dan alınıyor...";
            try
            {
                var sonuc = hepsiniYenidenAl
                    ? await _koordinatServisi.TumKoordinatlariYenidenAlAsync()
                    : await _koordinatServisi.EksikKoordinatlariTamamlaAsync();
                if (sonuc.Hata != null)
                    MessageBox.Show(sonuc.Hata, "Harita servisi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Ciz();
                lblDurum.Text = sonuc.Guncellenen + " konum güncellendi"
                              + (sonuc.Bulunamayan > 0 ? ", " + sonuc.Bulunamayan + " konum bulunamadı." : ".");
            }
            catch (Exception ex)
            {
                lblDurum.Text = "Koordinatlar alınamadı.";
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnKoordinat.Enabled = btnYenidenAl.Enabled = true;
            }
        }

        // Bölge özetlerini haritaya ve tabloya çizer
        private void Ciz()
        {
            _bolgeler = _haritaServisi.BolgeOzetleri();
            acikKatman.Markers.Clear();
            karsilananKatman.Markers.Clear();
            gonulluKatman.Markers.Clear();

            foreach (var b in _bolgeler.Where(x => x.KoordinatVar))
            {
                var nokta = new PointLatLng(b.Enlem.Value, b.Boylam.Value);
                int acik = b.Bekleyen + b.Surecte;

                if (acik > 0)
                {
                    var tur = b.KritikBekleyen > 0 ? GMarkerGoogleType.red_dot
                            : b.Bekleyen > 0 ? GMarkerGoogleType.orange_dot
                            : GMarkerGoogleType.blue_dot;
                    acikKatman.Markers.Add(new GMarkerGoogle(nokta, tur)
                    {
                        ToolTipText = b.Baslik + "\nBekleyen: " + b.Bekleyen + " (kritik " + b.KritikBekleyen + ")"
                                    + "\nSüreçte: " + b.Surecte + "\nKarşılanan: " + b.Karsilanan,
                        ToolTipMode = MarkerTooltipMode.OnMouseOver,
                        Tag = b.Baslik + " - açık talepler (en acil üstte)\n\n" + b.AcikTalepDetayi
                    });
                }
                else if (b.Karsilanan > 0)
                {
                    karsilananKatman.Markers.Add(new GMarkerGoogle(nokta, GMarkerGoogleType.gray_small)
                    {
                        ToolTipText = b.Baslik + "\nAçık talep yok\nKarşılanan: " + b.Karsilanan,
                        ToolTipMode = MarkerTooltipMode.OnMouseOver
                    });
                }

                if (b.GonulluToplam > 0)
                {
                    // Aynı noktadaki talep işaretinin üstüne binmesin diye biraz doğuya kaydırılır
                    gonulluKatman.Markers.Add(new GMarkerGoogle(new PointLatLng(nokta.Lat, nokta.Lng + 0.03), GMarkerGoogleType.green_small)
                    {
                        ToolTipText = b.Baslik + "\nGönüllü: " + b.GonulluToplam + " (" + b.GonulluMusait + " müsait)",
                        ToolTipMode = MarkerTooltipMode.OnMouseOver,
                        Tag = b.Baslik + " - gönüllüler\n\n" + b.GonulluDetayi
                    });
                }
            }

            dgvBolgeler.DataSource = _bolgeler.Select(b => new
            {
                Bolge = b.Baslik + (b.KoordinatVar ? "" : " (koordinat yok)"),
                Kritik = b.KritikBekleyen,
                Bekleyen = b.Bekleyen,
                Surecte = b.Surecte,
                Karsilanan = b.Karsilanan,
                Gonullu = b.GonulluMusait + "/" + b.GonulluToplam + " müsait"
            }).ToList();

            int koordinatsiz = _bolgeler.Count(b => !b.KoordinatVar);
            lblDurum.Text = _bolgeler.Count + " bölge"
                          + (koordinatsiz > 0 ? " (" + koordinatsiz + " bölge koordinatsız, haritada görünmez)" : "");
        }

        private void BolgeyeGit(int satir)
        {
            if (satir < 0 || satir >= _bolgeler.Count) return;
            var b = _bolgeler[satir];
            if (!b.KoordinatVar)
            {
                MessageBox.Show("Bu bölgenin koordinatı yok. \"Koordinatları Güncelle\"yi deneyin.", "Harita");
                return;
            }
            gMap.Position = new PointLatLng(b.Enlem.Value, b.Boylam.Value);
            if (gMap.Zoom < 12) gMap.Zoom = 12;
        }

        private static void LejantSatiri(Control kutu, int sira, Color renk, string metin)
        {
            int y = 22 + sira * 24;
            kutu.Controls.Add(new Panel { BackColor = renk, Location = new Point(10, y + 2), Size = new Size(14, 14) });
            kutu.Controls.Add(new Label { Text = metin, Location = new Point(30, y), AutoSize = true });
        }
    }
}
