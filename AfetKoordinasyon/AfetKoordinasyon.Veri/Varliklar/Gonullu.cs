using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Gonullu
    {
        public int GonulluId { get; set; }
        [Required, StringLength(100)] public string AdSoyad { get; set; }
        [Required, StringLength(15)] public string Telefon { get; set; }
        [StringLength(200)] public string Beceri { get; set; }

        public int KonumId { get; set; }
        public virtual Konum Konum { get; set; }
        public virtual ICollection<Eslesme> Eslesmeler { get; set; }
    }
}