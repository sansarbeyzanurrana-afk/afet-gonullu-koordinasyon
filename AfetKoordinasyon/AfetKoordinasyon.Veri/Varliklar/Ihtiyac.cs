using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Ihtiyac
    {
        public int IhtiyacId { get; set; }
        [Required, StringLength(50)] public string Tur { get; set; }
        [StringLength(500)] public string Aciklama { get; set; }
        public int Miktar { get; set; }
        public DateTime Tarih { get; set; }

        public int KonumId { get; set; }
        public virtual Konum Konum { get; set; }
        public int DurumId { get; set; }
        public virtual Durum Durum { get; set; }
        public int KategoriId { get; set; }
        public virtual Kategori Kategori { get; set; }
        public virtual ICollection<Eslesme> Eslesmeler { get; set; }
    }
}