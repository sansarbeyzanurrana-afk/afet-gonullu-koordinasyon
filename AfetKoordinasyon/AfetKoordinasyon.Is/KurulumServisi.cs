using AfetKoordinasyon.Veri;

namespace AfetKoordinasyon.Is
{
    // Uygulama açılışındaki kurulum işleri (Sunum katmanı Veri katmanına doğrudan erişmesin diye)
    public class KurulumServisi
    {
        public void VeritabaniniHazirla()
        {
            VeritabaniKurulumu.Hazirla();
        }
    }
}
