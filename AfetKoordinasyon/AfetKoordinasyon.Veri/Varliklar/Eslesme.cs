using System;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Eslesme
    {
        public int EslesmeId { get; set; }
        public DateTime Tarih { get; set; }

        // Atama kaldırılırsa ya da ihtiyaç iptal edilirse eşleşme silinmez, pasif yapılır (geçmiş korunur).
        // Bir ihtiyacın aynı anda yalnızca BİR aktif eşleşmesi olabilir (veritabanında benzersiz indeks).
        public bool Aktif { get; set; } = true;
        public DateTime? BitisTarihi { get; set; }

        public int IhtiyacId { get; set; }
        public virtual Ihtiyac Ihtiyac { get; set; }
        public int GonulluId { get; set; }
        public virtual Gonullu Gonullu { get; set; }
    }
}