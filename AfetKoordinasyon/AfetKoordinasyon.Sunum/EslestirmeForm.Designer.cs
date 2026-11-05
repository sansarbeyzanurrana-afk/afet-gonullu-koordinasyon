namespace AfetKoordinasyon.Sunum
{
    partial class EslestirmeForm
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
            this.dgvIhtiyaclar = new System.Windows.Forms.DataGridView();
            this.dgvGonulluler = new System.Windows.Forms.DataGridView();
            this.btnEslestir = new System.Windows.Forms.Button();
            this.dgvEslesmisler = new System.Windows.Forms.DataGridView();
            this.btnTamamla = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIhtiyaclar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGonulluler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEslesmisler)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvIhtiyaclar
            // 
            this.dgvIhtiyaclar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIhtiyaclar.Location = new System.Drawing.Point(12, 12);
            this.dgvIhtiyaclar.Name = "dgvIhtiyaclar";
            this.dgvIhtiyaclar.RowHeadersWidth = 51;
            this.dgvIhtiyaclar.RowTemplate.Height = 24;
            this.dgvIhtiyaclar.Size = new System.Drawing.Size(325, 150);
            this.dgvIhtiyaclar.TabIndex = 0;
            this.dgvIhtiyaclar.SelectionChanged += new System.EventHandler(this.dgvIhtiyaclar_SelectionChanged);
            // 
            // dgvGonulluler
            // 
            this.dgvGonulluler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGonulluler.Location = new System.Drawing.Point(458, 12);
            this.dgvGonulluler.Name = "dgvGonulluler";
            this.dgvGonulluler.RowHeadersWidth = 51;
            this.dgvGonulluler.Size = new System.Drawing.Size(330, 150);
            this.dgvGonulluler.TabIndex = 1;
            // 
            // btnEslestir
            // 
            this.btnEslestir.Location = new System.Drawing.Point(245, 208);
            this.btnEslestir.Name = "btnEslestir";
            this.btnEslestir.Size = new System.Drawing.Size(264, 86);
            this.btnEslestir.TabIndex = 2;
            this.btnEslestir.Text = "Eşleştir";
            this.btnEslestir.UseVisualStyleBackColor = true;
            this.btnEslestir.Click += new System.EventHandler(this.btnEslestir_Click);
            // 
            // dgvEslesmisler
            // 
            this.dgvEslesmisler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEslesmisler.Location = new System.Drawing.Point(22, 341);
            this.dgvEslesmisler.Name = "dgvEslesmisler";
            this.dgvEslesmisler.RowHeadersWidth = 51;
            this.dgvEslesmisler.RowTemplate.Height = 24;
            this.dgvEslesmisler.Size = new System.Drawing.Size(330, 181);
            this.dgvEslesmisler.TabIndex = 3;
            this.dgvEslesmisler.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // btnTamamla
            // 
            this.btnTamamla.Location = new System.Drawing.Point(424, 388);
            this.btnTamamla.Name = "btnTamamla";
            this.btnTamamla.Size = new System.Drawing.Size(157, 65);
            this.btnTamamla.TabIndex = 4;
            this.btnTamamla.Text = "Tamamla";
            this.btnTamamla.UseVisualStyleBackColor = true;
            this.btnTamamla.Click += new System.EventHandler(this.btnTamamla_Click);
            // 
            // EslestirmeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 534);
            this.Controls.Add(this.btnTamamla);
            this.Controls.Add(this.dgvEslesmisler);
            this.Controls.Add(this.btnEslestir);
            this.Controls.Add(this.dgvGonulluler);
            this.Controls.Add(this.dgvIhtiyaclar);
            this.Name = "EslestirmeForm";
            this.Text = "EslestirmeForm";
            this.Load += new System.EventHandler(this.EslestirmeForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIhtiyaclar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGonulluler)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEslesmisler)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvIhtiyaclar;
        private System.Windows.Forms.DataGridView dgvGonulluler;
        private System.Windows.Forms.Button btnEslestir;
        private System.Windows.Forms.DataGridView dgvEslesmisler;
        private System.Windows.Forms.Button btnTamamla;
    }
}