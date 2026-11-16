using System;
using System.ComponentModel.DataAnnotations;

namespace AfetKoordinasyon.Veri.Varliklar
{
    // Bir ihtiyacın her durum değişikliği burada saklanır (kim, ne zaman, neden).
    // Kayıtlar sadece eklenir; güncellenmez ve silinmez (denetim izi).
    public class DurumGecmisi
    {
        public int DurumGecmisiId { get; set; }

        public int IhtiyacId { get; set; }
        public virtual Ihtiyac Ihtiyac { get; set; }

        // Durum adları metin olarak saklanır: o anki durumun "fotoğrafı"
        [StringLength(30)] public string EskiDurum { get; set; }          // ilk kayıtta boş
        [Required, StringLength(30)] public string YeniDurum { get; set; }

        public DateTime Tarih { get; set; }

        public int? KullaniciId { get; set; }
        public virtual Kullanici Kullanici { get; set; }

        [StringLength(500)] public string Not { get; set; }
    }
}
