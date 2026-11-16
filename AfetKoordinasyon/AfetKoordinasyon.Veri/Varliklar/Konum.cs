using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Konum
    {
        public int KonumId { get; set; }
        [Required, StringLength(50)] public string Il { get; set; }
        [Required, StringLength(50)] public string Ilce { get; set; }
        [StringLength(100)] public string Mahalle { get; set; }

        // Harita için koordinatlar (LocationIQ'dan bir kez alınıp saklanır).
        // Henüz alınmadıysa boş (null) kalır.
        public double? Enlem { get; set; }
        public double? Boylam { get; set; }

        public virtual ICollection<Gonullu> Gonulluler { get; set; }
        public virtual ICollection<Ihtiyac> Ihtiyaclar { get; set; }
    }
}
