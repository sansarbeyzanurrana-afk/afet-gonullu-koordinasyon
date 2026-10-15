using System.Collections.Generic;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class KonumServisi
    {
        public List<Konum> Listele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Konumlar.OrderBy(k => k.Il).ThenBy(k => k.Ilce).ToList();
            }
        }
    }
}