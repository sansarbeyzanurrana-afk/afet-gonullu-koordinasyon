using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public class IhtiyacServisi
    {
        public List<Ihtiyac> Listele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Ihtiyaclar
                         .Include(i => i.Konum)
                         .Include(i => i.Kategori)
                         .Include(i => i.Durum)
                         .OrderByDescending(i => i.Tarih)
                         .ToList();
            }
        }

        public List<Ihtiyac> BekleyenleriListele()
        {
            using (var db = new AfetDbContext())
            {
                return db.Ihtiyaclar
                         .Include(i => i.Konum)
                         .Where(i => i.Durum.Ad == "Bekliyor")
                         .ToList();
            }
        }

        public void Ekle(Ihtiyac ihtiyac)
        {
            if (string.IsNullOrWhiteSpace(ihtiyac.Tur))
                throw new ArgumentException("İhtiyaç türü boş olamaz.");
            if (ihtiyac.Tur.Length > 50)
                throw new ArgumentException("İhtiyaç türü en fazla 50 karakter olabilir.");
            if (ihtiyac.Aciklama != null && ihtiyac.Aciklama.Length > 500)
                throw new ArgumentException("Açıklama en fazla 500 karakter olabilir.");
            if (ihtiyac.Miktar <= 0)
                throw new ArgumentException("Miktar 0'dan büyük olmalı.");

            using (var db = new AfetDbContext())
            {
                if (!db.Kategoriler.Any(k => k.Id == ihtiyac.KategoriId))
                    throw new ArgumentException("Geçersiz kategori.");
                if (!db.Konumlar.Any(k => k.KonumId == ihtiyac.KonumId))
                    throw new ArgumentException("Geçersiz konum.");

                ihtiyac.DurumId = db.Durumlar.Single(d => d.Ad == "Bekliyor").DurumId;
                ihtiyac.Tarih = DateTime.Now;
                ihtiyac.KullaniciId = Oturum.AktifKullanici.Id;
                db.Ihtiyaclar.Add(ihtiyac);
                db.SaveChanges();
            }
        }
        public void Guncelle(Ihtiyac ihtiyac)
        {
            if (string.IsNullOrWhiteSpace(ihtiyac.Tur))
                throw new ArgumentException("İhtiyaç türü boş olamaz.");
            if (ihtiyac.Miktar <= 0)
                throw new ArgumentException("Miktar 0'dan büyük olmalı.");

            using (var db = new AfetDbContext())
            {
                if (!db.Kategoriler.Any(k => k.Id == ihtiyac.KategoriId))
                    throw new ArgumentException("Geçersiz kategori.");
                var mevcut = db.Ihtiyaclar.Find(ihtiyac.IhtiyacId);
                if (mevcut == null)
                    throw new ArgumentException("İhtiyaç bulunamadı.");


                 if (Oturum.AktifKullanici.Rol != "Koordinator" && mevcut.KullaniciId != Oturum.AktifKullanici.Id)
                    throw new UnauthorizedAccessException("Sadece kendi eklediğiniz ihtiyacı güncelleyebilirsiniz.");
                mevcut.Tur = ihtiyac.Tur;
                mevcut.Aciklama = ihtiyac.Aciklama;
                mevcut.Miktar = ihtiyac.Miktar;
                mevcut.KonumId = ihtiyac.KonumId;
                mevcut.KategoriId = ihtiyac.KategoriId;
                db.SaveChanges();
            }
        }

        public void Sil(int ihtiyacId)
        {
            Oturum.RolGerekli("Koordinator");
            using (var db = new AfetDbContext())
            {
                if (db.Eslesmeler.Any(e => e.IhtiyacId == ihtiyacId))
                    throw new InvalidOperationException("Eşleşmesi olan ihtiyaç silinemez.");

                var ihtiyac = db.Ihtiyaclar.Find(ihtiyacId);
                if (ihtiyac == null)
                    throw new ArgumentException("İhtiyaç bulunamadı.");

                db.Ihtiyaclar.Remove(ihtiyac);
                db.SaveChanges();
            }
        }
    } 
}