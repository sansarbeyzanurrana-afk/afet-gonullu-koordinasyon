using System;
using System.Drawing;
using System.Windows.Forms;

namespace AfetKoordinasyon.Sunum
{
    // ORTAK GÖRÜNÜM (Hafta 14)
    // Tüm formlar aynı renkleri, yazı tipini, buton ve tablo stilini kullanır.
    // Koyu arka plan üzerinde beyaz yazı, beyaz kutular içinde koyu yazı: yüksek kontrast (okunabilirlik).
    // Arka plan görselleri projeye özel çizildi (telifsiz, gerçek kişi içermez).
    internal static class Tema
    {
        public static readonly Color KoyuLacivert = Color.FromArgb(15, 36, 64);
        public static readonly Color Lacivert = Color.FromArgb(23, 59, 92);
        public static readonly Color Turuncu = Color.FromArgb(232, 119, 46);      // ana eylem (Giriş, Ekle, Kaydet…)
        public static readonly Color TuruncuKoyu = Color.FromArgb(201, 96, 30);
        public static readonly Color Turkuaz = Color.FromArgb(42, 157, 143);      // yapay zekâ butonu
        public static readonly Color Kirmizi = Color.FromArgb(192, 57, 43);       // silme gibi geri alınamaz işlemler
        public static readonly Color Metin = Color.FromArgb(30, 41, 59);
        public static readonly Color SolukMetin = Color.FromArgb(100, 116, 139);
        public static readonly Color Cizgi = Color.FromArgb(203, 213, 225);
        public static readonly Color SatirAlt = Color.FromArgb(241, 245, 249);
        public static readonly Color Secili = Color.FromArgb(253, 226, 204);
        public static readonly Color AcikYazi = Color.FromArgb(214, 226, 238);

        public static Font Yazi(float boyut = 10f) { return new Font("Segoe UI", boyut); }
        public static Font Kalin(float boyut = 10f) { return new Font("Segoe UI Semibold", boyut); }

        // Formun arka planını ayarlar ve içindeki tüm kontrollere temayı uygular
        public static void Uygula(Form form, Image arkaPlan)
        {
            form.BackColor = KoyuLacivert;              // görsel yüklenemezse de koyu zemin
            form.BackgroundImage = arkaPlan;
            form.BackgroundImageLayout = ImageLayout.Stretch;
            Kontroller(form.Controls, koyuZemin: true);
        }

        // koyuZemin: true ise etiketler beyaz, false ise (beyaz kart içi) koyu yazılır
        public static void Kontroller(Control.ControlCollection kontroller, bool koyuZemin)
        {
            foreach (Control c in kontroller)
            {
                if (c is Label)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = koyuZemin ? Color.White : Metin;
                    c.Font = Kalin(10.5f);
                }
                else if (c is TextBox)
                {
                    var t = (TextBox)c;
                    t.BackColor = Color.White; t.ForeColor = Metin;
                    t.BorderStyle = BorderStyle.FixedSingle; t.Font = Yazi(10.5f);
                }
                else if (c is ComboBox)
                {
                    var cb = (ComboBox)c;
                    cb.BackColor = Color.White; cb.ForeColor = Metin;
                    cb.FlatStyle = FlatStyle.Flat; cb.Font = Yazi(10f);
                }
                else if (c is NumericUpDown)
                {
                    c.BackColor = Color.White; c.ForeColor = Metin; c.Font = Yazi(10.5f);
                }
                else if (c is Button)
                {
                    ButonIkincil((Button)c);
                }
                else if (c is DataGridView)
                {
                    Tablo((DataGridView)c);
                }
                else if (c.HasChildren)
                {
                    Kontroller(c.Controls, koyuZemin);
                }
            }
        }

        // ---------------- Butonlar ----------------
        private static void Buton(Button b, Color zemin, Color yazi, Color uzerinde, int kenar, Color kenarRengi, float boyut)
        {
            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = zemin;
            b.ForeColor = yazi;
            b.FlatAppearance.BorderSize = kenar;
            b.FlatAppearance.BorderColor = kenarRengi;
            b.FlatAppearance.MouseOverBackColor = uzerinde;
            b.Font = Kalin(boyut);
            b.Cursor = Cursors.Hand;

            // Pasif buton gri görünsün: kullanıcı tıklanamayan butonu aktif sanmasın (Hafta 15 düzeltmesi)
            b.Tag = new[] { zemin, yazi, kenarRengi };
            b.EnabledChanged -= PasifGorunum;
            b.EnabledChanged += PasifGorunum;
            PasifGorunum(b, EventArgs.Empty);
        }

        private static readonly Color PasifZemin = Color.FromArgb(226, 232, 240);
        private static readonly Color PasifYazi = Color.FromArgb(148, 163, 184);

        private static void PasifGorunum(object sender, EventArgs e)
        {
            var b = (Button)sender;
            var renkler = b.Tag as Color[];
            if (renkler == null) return;
            b.BackColor = b.Enabled ? renkler[0] : PasifZemin;
            b.ForeColor = b.Enabled ? renkler[1] : PasifYazi;
            b.FlatAppearance.BorderColor = b.Enabled ? renkler[2] : PasifZemin;
            b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
        }

        public static void ButonAna(Button b, float boyut = 10f)     { Buton(b, Turuncu, Color.White, TuruncuKoyu, 0, Turuncu, boyut); }
        public static void ButonIkincil(Button b, float boyut = 10f) { Buton(b, Color.White, Lacivert, SatirAlt, 1, Cizgi, boyut); }
        public static void ButonTehlike(Button b, float boyut = 10f) { Buton(b, Kirmizi, Color.White, Color.FromArgb(160, 45, 34), 0, Kirmizi, boyut); }
        public static void ButonVurgu(Button b, float boyut = 10f)   { Buton(b, Turkuaz, Color.White, Color.FromArgb(33, 128, 117), 0, Turkuaz, boyut); }

        // ---------------- Tablolar ----------------
        public static void Tablo(DataGridView g, float boyut = 10f)
        {
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Cizgi;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 34;
            g.ColumnHeadersDefaultCellStyle.BackColor = Lacivert;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Lacivert;
            g.ColumnHeadersDefaultCellStyle.Font = Kalin(boyut);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.Font = Yazi(boyut);
            g.DefaultCellStyle.ForeColor = Metin;
            g.DefaultCellStyle.SelectionBackColor = Secili;
            g.DefaultCellStyle.SelectionForeColor = Metin;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = SatirAlt;
            g.RowTemplate.Height = 30;
        }

        // ---------------- Veri yoğun ekranlar (Durum Takibi, Harita, Rapor) ----------------
        // Bu ekranlarda okunabilirlik için açık zemin kullanılır; üst araç çubuğu lacivert olur.
        public static void UygulaAcik(Form form)
        {
            form.BackColor = Color.FromArgb(245, 247, 250);
            AcikKontroller(form.Controls);
        }

        private static void AcikKontroller(Control.ControlCollection kontroller)
        {
            foreach (Control c in kontroller)
            {
                if (c is Panel && c.Dock == DockStyle.Top && c.BackColor == Color.LemonChiffon)
                {
                    // Üst araç çubuğu: lacivert zemin, beyaz yazı
                    c.BackColor = Lacivert;
                    foreach (Control k in c.Controls)
                    {
                        if (k is Label || k is CheckBox) { k.ForeColor = Color.White; k.BackColor = Color.Transparent; }
                        else if (k is Button) ButonIkincil((Button)k, 9.5f);
                    }
                }
                else if (c is Button)
                {
                    var b = (Button)c;
                    bool tehlike = b.ForeColor == Color.DarkRed;      // ör. İptal Et
                    ButonIkincil(b, 9.5f);
                    if (tehlike)
                    {
                        ((Color[])b.Tag)[1] = Kirmizi;
                        if (b.Enabled) b.ForeColor = Kirmizi;
                    }
                }
                else if (c is GroupBox)
                {
                    c.ForeColor = Lacivert;
                    AcikKontroller(c.Controls);
                }
                else if (c is DataGridView)
                {
                    Tablo((DataGridView)c, 9.5f);
                }
                else if (c.HasChildren)
                {
                    AcikKontroller(c.Controls);
                }
            }
        }

        // ---------------- Kart (beyaz kutu) ----------------
        // İçine sırayla eklenen kontrolleri alt alta dizer; yazı büyüse de taşmaz (AutoSize)
        public static TableLayoutPanel Kart(int genislik)
        {
            var kart = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(30, 26, 30, 26)
            };
            kart.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, genislik));
            return kart;
        }

        public static void KartaEkle(TableLayoutPanel kart, Control c, int ustBosluk = 0)
        {
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0, ustBosluk, 0, 4);
            kart.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            kart.Controls.Add(c, 0, kart.RowCount++);
        }

        public static Label Baslik(string metin, float boyut, Color renk)
        {
            return new Label { Text = metin, AutoSize = true, BackColor = Color.Transparent, ForeColor = renk, Font = Kalin(boyut) };
        }
    }
}
