using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfetKoordinasyon.Veri.Varliklar
{
    [Table("Kullanicilar")]
    public class Kullanici
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string AdSoyad { get; set; }

        [Required, StringLength(100)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string ParolaHash { get; set; }

        [Required, StringLength(20)]
        public string Rol { get; set; }

        public int HataliGiris { get; set; }
        public DateTime? KilitBitis { get; set; }
    }
}