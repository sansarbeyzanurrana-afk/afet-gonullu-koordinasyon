using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AfetKoordinasyon.Is;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Sunum
{
    // GÖNÜLLÜ YÖNETİMİ (Hafta 15 – son entegrasyon)
    // Koordinatör gönüllüleri listeler ve yeni gönüllü ekler. Kurallar GonulluServisi'nde.
    public class GonulluForm : Form
    {
        private readonly GonulluServisi _gonulluServisi = new GonulluServisi();
        private readonly KonumServisi _konumServisi = new KonumServisi();

        private readonly DataGridView dgvGonulluler = new DataGridView();
        private readonly TextBox txtAdSoyad = new TextBox();
        private readonly TextBox txtTelefon = new TextBox();
        private readonly TextBox txtBeceri = new TextBox();
        private readonly ComboBox cmbKonum = new ComboBox();
        private readonly Button btnEkle = new Button();
        private readonly Label lblOzet = new Label();

        public GonulluForm()
        {
            Text = "Gönüllü Yönetimi";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(940, 560);
            MinimumSize = new Size(780, 460);
            Font = new Font("Segoe UI", 9F);

            // Üst bilgi çubuğu
            var ust = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.LemonChiffon };
            lblOzet.AutoSize = true; lblOzet.Location = new Point(10, 13);
            ust.Controls.Add(lblOzet);

            // Sağda ekleme formu
            var kutu = new GroupBox { Text = "Yeni gönüllü ekle", Dock = DockStyle.Right, Width = 310 };
            int y = 30;
            Alan(kutu, "Ad Soyad", txtAdSoyad, ref y);
            Alan(kutu, "Telefon (05 ile başlayan 11 hane)", txtTelefon, ref y);
            Alan(kutu, "Beceri (ör. İlk yardım, Lojistik)", txtBeceri, ref y);
            Alan(kutu, "Konum", cmbKonum, ref y);
            btnEkle.Text = "Gönüllü Ekle";
            btnEkle.Location = new Point(14, y + 6);
            btnEkle.Size = new Size(278, 38);
            kutu.Controls.Add(btnEkle);
            kutu.Controls.Add(new Label
            {
                Text = "Yeni gönüllü \"Müsait\" olarak eklenir ve aynı konumdaki ihtiyaçlar için eşleştirmede listelenir.\n\nDemo ve testlerde yalnızca uydurma veri kullanın (KVKK).",
                Location = new Point(14, y + 56), Size = new Size(278, 110), ForeColor = Color.DimGray
            });

            // Solda liste
            dgvGonulluler.Dock = DockStyle.Fill;
            dgvGonulluler.ReadOnly = true;
            dgvGonulluler.AllowUserToAddRows = false;
            dgvGonulluler.AllowUserToDeleteRows = false;
            dgvGonulluler.MultiSelect = false;
            dgvGonulluler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGonulluler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGonulluler.AccessibleName = "Gönüllü listesi";

            Controls.Add(dgvGonulluler);
            Controls.Add(kutu);
            Controls.Add(ust);
            Tema.UygulaAcik(this);
            Tema.ButonAna(btnEkle);

            // Kullanılabilirlik
            cmbKonum.DropDownStyle = ComboBoxStyle.DropDownList;
            txtTelefon.MaxLength = 11;
            txtTelefon.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };  // sadece rakam
            AcceptButton = btnEkle;
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            Load += (s, e) => { KonumlariYukle(); Yukle(); txtAdSoyad.Focus(); };
            btnEkle.Click += (s, e) => Ekle();
        }

        private static void Alan(Control kutu, string etiket, Control giris, ref int y)
        {
            kutu.Controls.Add(new Label { Text = etiket, AutoSize = true, Location = new Point(14, y) });
            giris.Location = new Point(14, y + 22);
            giris.Width = 278;
            giris.AccessibleName = etiket;
            kutu.Controls.Add(giris);
            y += 62;
        }

        private void KonumlariYukle()
        {
            cmbKonum.DataSource = _konumServisi.Listele()
                .Select(k => new { k.KonumId, Ad = k.Il + " / " + k.Ilce })
                .ToList();
            cmbKonum.DisplayMember = "Ad";
            cmbKonum.ValueMember = "KonumId";
        }

        private void Yukle()
        {
            var konumlar = _konumServisi.Listele().ToDictionary(k => k.KonumId, k => k.Il + " / " + k.Ilce);
            var liste = _gonulluServisi.Listele();
            dgvGonulluler.DataSource = liste
                .Select(g => new
                {
                    No = g.GonulluId,
                    g.AdSoyad,
                    g.Telefon,
                    Beceri = g.Beceri ?? "-",
                    Konum = konumlar.TryGetValue(g.KonumId, out var ad) ? ad : "-",
                    Durum = g.MusaitMi ? "Müsait" : "Görevde"
                })
                .ToList();
            lblOzet.Text = "Toplam " + liste.Count + " gönüllü · " + liste.Count(g => g.MusaitMi) + " müsait · "
                         + liste.Count(g => !g.MusaitMi) + " görevde";
        }

        private void Ekle()
        {
            try
            {
                _gonulluServisi.Ekle(new Gonullu
                {
                    AdSoyad = txtAdSoyad.Text,
                    Telefon = txtTelefon.Text,
                    Beceri = txtBeceri.Text,
                    KonumId = cmbKonum.SelectedValue is int id ? id : 0
                });
                MessageBox.Show("Gönüllü eklendi.", "Gönüllü Yönetimi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAdSoyad.Clear(); txtTelefon.Clear(); txtBeceri.Clear();
                txtAdSoyad.Focus();
                Yukle();
            }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Gönüllü Yönetimi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Gönüllü Yönetimi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message, "Yetki", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
