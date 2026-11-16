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

        // Yapay zekâ bileşeni (Hafta 10)
        // Aciliyet: 1 = düşük ... 5 = hayati. Henüz değerlendirilmediyse boş (null).
        [Range(1, 5)] public int? AciliyetSkoru { get; set; }
        // AI'ın (ya da yedek kuralların) bu kararı neden verdiği; şeffaflık için saklanır
        [StringLength(300)] public string AiGerekce { get; set; }

        public int KonumId { get; set; }
        public virtual Konum Konum { get; set; }
        public int DurumId { get; set; }
        public virtual Durum Durum { get; set; }
        public int KategoriId { get; set; }
        public virtual Kategori Kategori { get; set; }
        public int? KullaniciId { get; set; }
        public virtual Kullanici Kullanici { get; set; }
        public virtual ICollection<Eslesme> Eslesmeler { get; set; }
    }
}