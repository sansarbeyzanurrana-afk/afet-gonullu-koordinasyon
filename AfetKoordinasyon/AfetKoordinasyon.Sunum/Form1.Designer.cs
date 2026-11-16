namespace AfetKoordinasyon.Sunum
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtTur = new System.Windows.Forms.TextBox();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.nudMiktar = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbKonum = new System.Windows.Forms.ComboBox();
            this.btnEkle = new System.Windows.Forms.Button();
            this.dgvIhtiyaclar = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGuncelle = new System.Windows.Forms.Button();
            this.btnSil = new System.Windows.Forms.Button();
            this.btnCikis = new System.Windows.Forms.Button();
            this.cmbKategori = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnEslestirme = new System.Windows.Forms.Button();
            this.btnHarita = new System.Windows.Forms.Button();
            this.btnAiOner = new System.Windows.Forms.Button();
            this.btnDurumTakip = new System.Windows.Forms.Button();
            this.lblAiSonuc = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudMiktar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIhtiyaclar)).BeginInit();
            this.SuspendLayout();
            // 
            // txtTur
            // 
            this.txtTur.BackColor = System.Drawing.SystemColors.Info;
            this.txtTur.Location = new System.Drawing.Point(127, 12);
            this.txtTur.Name = "txtTur";
            this.txtTur.Size = new System.Drawing.Size(177, 22);
            this.txtTur.TabIndex = 0;
            // 
            // txtAciklama
            // 
            this.txtAciklama.BackColor = System.Drawing.SystemColors.Info;
            this.txtAciklama.Location = new System.Drawing.Point(127, 56);
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(177, 22);
            this.txtAciklama.TabIndex = 1;
            this.txtAciklama.TextChanged += new System.EventHandler(this.txtAciklama_TextChanged);
            // 
            // nudMiktar
            // 
            this.nudMiktar.BackColor = System.Drawing.SystemColors.Info;
            this.nudMiktar.Location = new System.Drawing.Point(113, 106);
            this.nudMiktar.Name = "nudMiktar";
            this.nudMiktar.Size = new System.Drawing.Size(191, 22);
            this.nudMiktar.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Info;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "Tür";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.Info;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(8, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "Açıklama";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Info;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "Miktar";
            // 
            // cmbKonum
            // 
            this.cmbKonum.BackColor = System.Drawing.SystemColors.Info;
            this.cmbKonum.FormattingEnabled = true;
            this.cmbKonum.Location = new System.Drawing.Point(113, 162);
            this.cmbKonum.Name = "cmbKonum";
            this.cmbKonum.Size = new System.Drawing.Size(191, 24);
            this.cmbKonum.TabIndex = 6;
            // 
            // btnEkle
            // 
            this.btnEkle.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnEkle.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnEkle.Location = new System.Drawing.Point(413, 104);
            this.btnEkle.Name = "btnEkle";
            this.btnEkle.Size = new System.Drawing.Size(75, 36);
            this.btnEkle.TabIndex = 7;
            this.btnEkle.Text = "Ekle";
            this.btnEkle.UseVisualStyleBackColor = false;
            // 
            // dgvIhtiyaclar
            // 
            this.dgvIhtiyaclar.BackgroundColor = System.Drawing.Color.MintCream;
            this.dgvIhtiyaclar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIhtiyaclar.Location = new System.Drawing.Point(15, 295);
            this.dgvIhtiyaclar.Name = "dgvIhtiyaclar";
            this.dgvIhtiyaclar.RowHeadersWidth = 51;
            this.dgvIhtiyaclar.RowTemplate.Height = 24;
            this.dgvIhtiyaclar.Size = new System.Drawing.Size(907, 229);
            this.dgvIhtiyaclar.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Info;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(8, 162);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 24);
            this.label4.TabIndex = 9;
            this.label4.Text = "Konum";
            // 
            // btnGuncelle
            // 
            this.btnGuncelle.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.btnGuncelle.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnGuncelle.Location = new System.Drawing.Point(413, 159);
            this.btnGuncelle.Name = "btnGuncelle";
            this.btnGuncelle.Size = new System.Drawing.Size(87, 35);
            this.btnGuncelle.TabIndex = 10;
            this.btnGuncelle.Text = "Güncelle";
            this.btnGuncelle.UseVisualStyleBackColor = false;
            this.btnGuncelle.Click += new System.EventHandler(this.btnGuncelle_Click);
            // 
            // btnSil
            // 
            this.btnSil.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnSil.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnSil.Location = new System.Drawing.Point(413, 221);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(75, 39);
            this.btnSil.TabIndex = 11;
            this.btnSil.Text = "Sil";
            this.btnSil.UseVisualStyleBackColor = false;
            this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
            // 
            // btnCikis
            // 
            this.btnCikis.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnCikis.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnCikis.Location = new System.Drawing.Point(413, 51);
            this.btnCikis.Name = "btnCikis";
            this.btnCikis.Size = new System.Drawing.Size(75, 31);
            this.btnCikis.TabIndex = 12;
            this.btnCikis.Text = "Çıkış";
            this.btnCikis.UseVisualStyleBackColor = false;
            this.btnCikis.Click += new System.EventHandler(this.btnCikis_Click);
            // 
            // cmbKategori
            // 
            this.cmbKategori.BackColor = System.Drawing.Color.LemonChiffon;
            this.cmbKategori.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKategori.FormattingEnabled = true;
            this.cmbKategori.Location = new System.Drawing.Point(114, 224);
            this.cmbKategori.Name = "cmbKategori";
            this.cmbKategori.Size = new System.Drawing.Size(190, 24);
            this.cmbKategori.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.Info;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.Location = new System.Drawing.Point(8, 224);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 24);
            this.label5.TabIndex = 14;
            this.label5.Text = "Kategori";
            // 
            // btnEslestirme
            // 
            this.btnEslestirme.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnEslestirme.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnEslestirme.Location = new System.Drawing.Point(613, 134);
            this.btnEslestirme.Name = "btnEslestirme";
            this.btnEslestirme.Size = new System.Drawing.Size(264, 44);
            this.btnEslestirme.TabIndex = 15;
            this.btnEslestirme.Text = "Eşleştirme";
            this.btnEslestirme.UseVisualStyleBackColor = false;
            this.btnEslestirme.Click += new System.EventHandler(this.btnEslestirme_Click);
            // 
            // btnHarita
            // 
            this.btnHarita.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnHarita.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnHarita.Location = new System.Drawing.Point(613, 196);
            this.btnHarita.Name = "btnHarita";
            this.btnHarita.Size = new System.Drawing.Size(264, 44);
            this.btnHarita.TabIndex = 16;
            this.btnHarita.Text = "Harita";
            this.btnHarita.UseVisualStyleBackColor = false;
            this.btnHarita.Click += new System.EventHandler(this.btnHarita_Click);
            // 
            // btnDurumTakip
            // 
            this.btnDurumTakip.BackColor = System.Drawing.Color.LemonChiffon;
            this.btnDurumTakip.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnDurumTakip.Location = new System.Drawing.Point(613, 72);
            this.btnDurumTakip.Name = "btnDurumTakip";
            this.btnDurumTakip.Size = new System.Drawing.Size(264, 44);
            this.btnDurumTakip.TabIndex = 19;
            this.btnDurumTakip.Text = "Durum Takibi";
            this.btnDurumTakip.UseVisualStyleBackColor = false;
            this.btnDurumTakip.Click += new System.EventHandler(this.btnDurumTakip_Click);
            // 
            // btnAiOner
            // 
            this.btnAiOner.BackColor = System.Drawing.Color.LightSkyBlue;
            this.btnAiOner.Font = new System.Drawing.Font("Microsoft YaHei", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnAiOner.Location = new System.Drawing.Point(310, 52);
            this.btnAiOner.Name = "btnAiOner";
            this.btnAiOner.Size = new System.Drawing.Size(100, 30);
            this.btnAiOner.TabIndex = 17;
            this.btnAiOner.Text = "AI ile Öner";
            this.btnAiOner.UseVisualStyleBackColor = false;
            this.btnAiOner.Click += new System.EventHandler(this.btnAiOner_Click);
            // 
            // lblAiSonuc
            // 
            this.lblAiSonuc.BackColor = System.Drawing.Color.LemonChiffon;
            this.lblAiSonuc.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAiSonuc.Location = new System.Drawing.Point(500, 244);
            this.lblAiSonuc.Name = "lblAiSonuc";
            this.lblAiSonuc.Size = new System.Drawing.Size(422, 50);
            this.lblAiSonuc.TabIndex = 18;
            this.lblAiSonuc.Text = "Açıklamayı yazıp \"AI ile Öner\"e basın.";
            this.lblAiSonuc.Visible = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.BackgroundImage = global::AfetKoordinasyon.Sunum.Properties.Resources.deprem;
            this.ClientSize = new System.Drawing.Size(986, 553);
            this.Controls.Add(this.lblAiSonuc);
            this.Controls.Add(this.btnAiOner);
            this.Controls.Add(this.btnDurumTakip);
            this.Controls.Add(this.btnHarita);
            this.Controls.Add(this.btnEslestirme);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbKategori);
            this.Controls.Add(this.btnCikis);
            this.Controls.Add(this.btnSil);
            this.Controls.Add(this.btnGuncelle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgvIhtiyaclar);
            this.Controls.Add(this.btnEkle);
            this.Controls.Add(this.cmbKonum);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudMiktar);
            this.Controls.Add(this.txtAciklama);
            this.Controls.Add(this.txtTur);
            this.Name = "Form1";
            this.Text = "İhtiyaç Yönetimi";
            ((System.ComponentModel.ISupportInitialize)(this.nudMiktar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIhtiyaclar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTur;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.NumericUpDown nudMiktar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbKonum;
        private System.Windows.Forms.Button btnEkle;
        private System.Windows.Forms.DataGridView dgvIhtiyaclar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGuncelle;
        private System.Windows.Forms.Button btnSil;
        private System.Windows.Forms.Button btnCikis;
        private System.Windows.Forms.ComboBox cmbKategori;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnEslestirme;
        private System.Windows.Forms.Button btnHarita;
        private System.Windows.Forms.Button btnDurumTakip;
        private System.Windows.Forms.Button btnAiOner;
        private System.Windows.Forms.Label lblAiSonuc;
    }
}

