using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Durum
    {
        public int DurumId { get; set; }
        [Required, StringLength(30)] public string Ad { get; set; }

        public virtual ICollection<Ihtiyac> Ihtiyaclar { get; set; }
    }
}