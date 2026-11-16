using System;
using System.Drawing;
using System.Windows.Forms;
using AfetKoordinasyon.Is;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace AfetKoordinasyon.Sunum
{
    // İhtiyaçları (kırmızı) ve gönüllüleri (yeşil) harita üzerinde gösterir.
    public class HaritaForm : Form
    {
        private readonly HaritaServisi _haritaServisi = new HaritaServisi();
        private readonly KoordinatServisi _koordinatServisi = new KoordinatServisi();

        private readonly GMapControl gMap = new GMapControl();
        private readonly GMapOverlay ihtiyacKatmani = new GMapOverlay("ihtiyaclar");
        private readonly GMapOverlay gonulluKatmani = new GMapOverlay("gonulluler");

        private readonly Button btnKoordinat = new Button();
        private readonly CheckBox chkIhtiyac = new CheckBox();
        private readonly CheckBox chkGonullu = new CheckBox();
        private readonly Label lblDurum = new Label();
        private readonly Label lblKaynak = new Label();

        // Bingöl merkez (harita ilk açıldığında buraya bakar)
        private static readonly PointLatLng Baslangic = new PointLatLng(38.885, 40.498);

        public HaritaForm()
        {
            Text = "Harita - İhtiyaçlar ve Gönüllüler";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;

            // --- Üst panel: butonlar ve filtreler ---
            var ustPanel = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.LemonChiffon };

            btnKoordinat.Text = "Koordinatları Güncelle";
            btnKoordinat.Location = new Point(10, 10);
            btnKoordinat.Size = new Size(190, 28);
            btnKoordinat.Click += btnKoordinat_Click;

            chkIhtiyac.Text = "İhtiyaçlar (kırmızı)";
            chkIhtiyac.Checked = true;
            chkIhtiyac.AutoSize = true;
            chkIhtiyac.Location = new Point(215, 15);
            chkIhtiyac.CheckedChanged += (s, e) => ihtiyacKatmani.IsVisibile = chkIhtiyac.Checked;

            chkGonullu.Text = "Gönüllüler (yeşil)";
            chkGonullu.Checked = true;
            chkGonullu.AutoSize = true;
            chkGonullu.Location = new Point(365, 15);
            chkGonullu.CheckedChanged += (s, e) => gonulluKatmani.IsVisibile = chkGonullu.Checked;

            lblDurum.AutoSize = true;
            lblDurum.Location = new Point(510, 16);

            ustPanel.Controls.Add(btnKoordinat);
            ustPanel.Controls.Add(chkIhtiyac);
            ustPanel.Controls.Add(chkGonullu);
            ustPanel.Controls.Add(lblDurum);

            // --- Alt bilgi: veri kaynakları (LocationIQ ücretsiz planı bağlantı istiyor) ---
            lblKaynak.Dock = DockStyle.Bottom;
            lblKaynak.Height = 22;
            lblKaynak.TextAlign = ContentAlignment.MiddleRight;
            lblKaynak.Text = "Harita: © OpenStreetMap katkıcıları  |  Search by LocationIQ.com  ";

            // --- Harita ---
            gMap.Dock = DockStyle.Fill;
            gMap.MapProvider = GMapProviders.OpenStreetMap;
            gMap.Position = Baslangic;
            gMap.MinZoom = 3;
            gMap.MaxZoom = 18;
            gMap.Zoom = 9;
            gMap.DragButton = MouseButtons.Left;
            gMap.ShowCenter = false;
            gMap.Overlays.Add(ihtiyacKatmani);
            gMap.Overlays.Add(gonulluKatmani);
            gMap.OnMarkerClick += gMap_OnMarkerClick;

            Controls.Add(gMap);
            Controls.Add(ustPanel);
            Controls.Add(lblKaynak);

            Load += HaritaForm_Load;
        }

        private async void HaritaForm_Load(object sender, EventArgs e)
        {
            // Harita karoları internetten indirilir, ayrıca bilgisayarda önbelleğe alınır
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            // OpenStreetMap, isteği yapan uygulamanın kendini tanıtmasını istiyor
            GMapProvider.UserAgent = "AfetKoordinasyon/1.0 (Bingol Universitesi ogrenci projesi)";

            IsaretleriCiz();

            // Koordinatı eksik konum varsa API'den otomatik tamamla
            if (_haritaServisi.KoordinatiEksikKonumSayisi() > 0)
                await KoordinatlariGuncelleAsync();
        }

        private async void btnKoordinat_Click(object sender, EventArgs e)
        {
            await KoordinatlariGuncelleAsync();
        }

        private async System.Threading.Tasks.Task KoordinatlariGuncelleAsync()
        {
            btnKoordinat.Enabled = false;
            lblDurum.Text = "Koordinatlar LocationIQ'dan alınıyor...";
            try
            {
                var sonuc = await _koordinatServisi.EksikKoordinatlariTamamlaAsync();

                if (sonuc.Hata != null)
                {
                    MessageBox.Show(sonuc.Hata, "Harita servisi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                lblDurum.Text = sonuc.Guncellenen + " konum güncellendi"
                              + (sonuc.Bulunamayan > 0 ? ", " + sonuc.Bulunamayan + " konum bulunamadı." : ".");
                IsaretleriCiz();
            }
            catch (Exception ex)
            {
                // Beklenmeyen bir hata olsa bile form kapanmaz
                lblDurum.Text = "Koordinatlar alınamadı.";
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnKoordinat.Enabled = true;
            }
        }

        // Veritabanındaki noktaları haritaya işaret olarak ekler
        private void IsaretleriCiz()
        {
            ihtiyacKatmani.Markers.Clear();
            gonulluKatmani.Markers.Clear();

            foreach (var n in _haritaServisi.IhtiyacNoktalari())
            {
                var isaret = new GMarkerGoogle(new PointLatLng(n.Enlem, n.Boylam), GMarkerGoogleType.red_dot)
                {
                    ToolTipText = n.Baslik + ": " + n.Adet + " ihtiyaç",
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    Tag = n.Baslik + " - İhtiyaçlar\n\n" + n.Detay
                };
                ihtiyacKatmani.Markers.Add(isaret);
            }

            foreach (var n in _haritaServisi.GonulluNoktalari())
            {
                // Aynı konumdaki ihtiyaç işaretinin üstüne binmesin diye biraz doğuya kaydırıyoruz
                var isaret = new GMarkerGoogle(new PointLatLng(n.Enlem, n.Boylam + 0.01), GMarkerGoogleType.green_dot)
                {
                    ToolTipText = n.Baslik + ": " + n.Adet + " gönüllü",
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    Tag = n.Baslik + " - Gönüllüler\n\n" + n.Detay
                };
                gonulluKatmani.Markers.Add(isaret);
            }

            if (ihtiyacKatmani.Markers.Count == 0 && gonulluKatmani.Markers.Count == 0)
                lblDurum.Text = "Haritada gösterilecek konum yok (koordinatlar henüz alınmamış olabilir).";
        }

        // İşarete tıklanınca o konumdaki kayıtların ayrıntısını göster
        private void gMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (item.Tag is string detay)
                MessageBox.Show(detay, "Konum ayrıntısı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
