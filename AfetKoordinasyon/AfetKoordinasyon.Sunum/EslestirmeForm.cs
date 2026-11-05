using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AfetKoordinasyon.Is;

namespace AfetKoordinasyon.Sunum
{ 
public partial class EslestirmeForm : Form
{
    private readonly EslesmeServisi _servis = new EslesmeServisi();

    public EslestirmeForm()
    {
        InitializeComponent();
    }

    private void EslestirmeForm_Load(object sender, EventArgs e)
    {
        IhtiyaclariYukle();
        EslesmisleriYukle();
    }
        private void EslesmisleriYukle()
        {
            dgvEslesmisler.DataSource = _servis.EslesmisIhtiyaclariGetir()
                .Select(i => new { i.IhtiyacId, i.Tur, i.Aciklama, i.Miktar, i.Tarih })
                .ToList();

            dgvEslesmisler.ReadOnly = true;
            dgvEslesmisler.AllowUserToAddRows = false;
            dgvEslesmisler.MultiSelect = false;
            dgvEslesmisler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void IhtiyaclariYukle()
    {
        dgvIhtiyaclar.DataSource = _servis.BekleyenIhtiyaclariGetir()
            .Select(i => new { i.IhtiyacId, i.Tur, i.Aciklama, i.Miktar, i.Tarih })
            .ToList();

        dgvIhtiyaclar.ReadOnly = true;
        dgvIhtiyaclar.AllowUserToAddRows = false;
        dgvIhtiyaclar.MultiSelect = false;
        dgvIhtiyaclar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }


        private void dgvIhtiyaclar_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIhtiyaclar.CurrentRow == null)
            {
                dgvGonulluler.DataSource = null;
                return;
            }

            int ihtiyacId = (int)dgvIhtiyaclar.CurrentRow.Cells["IhtiyacId"].Value;

            dgvGonulluler.DataSource = _servis.UygunGonulluleriGetir(ihtiyacId)
                .Select(g => new { g.GonulluId, g.AdSoyad, g.Telefon, g.Beceri })
                .ToList();

            dgvGonulluler.ReadOnly = true;
            dgvGonulluler.AllowUserToAddRows = false;
            dgvGonulluler.MultiSelect = false;
            dgvGonulluler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

       
        private void btnEslestir_Click(object sender, EventArgs e)
        {
            if (dgvIhtiyaclar.CurrentRow == null || dgvGonulluler.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir ihtiyaç ve bir gönüllü seçin.", "Eşleştirme");
                return;
            }

            int ihtiyacId = (int)dgvIhtiyaclar.CurrentRow.Cells["IhtiyacId"].Value;
            int gonulluId = (int)dgvGonulluler.CurrentRow.Cells["GonulluId"].Value;

            try
            {
                _servis.Eslestir(ihtiyacId, gonulluId);
                MessageBox.Show("Eşleştirme yapıldı.", "Eşleştirme");
                IhtiyaclariYukle();
                EslesmisleriYukle();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Yetki");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Eşleştirme");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnTamamla_Click(object sender, EventArgs e)
        {
            if (dgvEslesmisler.CurrentRow == null)
            {
                MessageBox.Show("Lütfen tamamlanacak bir ihtiyaç seçin.", "Tamamla");
                return;
            }

            int ihtiyacId = (int)dgvEslesmisler.CurrentRow.Cells["IhtiyacId"].Value;

            try
            {
                _servis.Tamamla(ihtiyacId);
                MessageBox.Show("İhtiyaç tamamlandı, gönüllü tekrar müsait.", "Tamamla");
                IhtiyaclariYukle();
                EslesmisleriYukle();
               
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Yetki");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Tamamla");
            }
        }
    }
    }


