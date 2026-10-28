using System.Collections.Generic;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class KategoriServisi
    {
        public List<Kategori> Listele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Kategoriler.OrderBy(k => k.Ad).ToList();
            }
        }
    }
}