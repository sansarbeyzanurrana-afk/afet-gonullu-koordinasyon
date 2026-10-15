using System;

namespace AfetKoordinasyon.Veri.Varliklar
{
    public class Eslesme
    {
        public int EslesmeId { get; set; }
        public DateTime Tarih { get; set; }

        public int IhtiyacId { get; set; }
        public virtual Ihtiyac Ihtiyac { get; set; }
        public int GonulluId { get; set; }
        public virtual Gonullu Gonullu { get; set; }
    }
}