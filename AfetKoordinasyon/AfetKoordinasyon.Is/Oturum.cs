using System;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    public static class Oturum
    {
        private static readonly TimeSpan Sure = TimeSpan.FromMinutes(30);
        private static DateTime sonIslem;

        public static Kullanici AktifKullanici { get; private set; }

        public static void Baslat(Kullanici k)
        {
            AktifKullanici = k;
            sonIslem = DateTime.Now;
        }

        public static void Cikis()
        {
            AktifKullanici = null;
        }

        public static bool Aktif
        {
            get
            {
                if (AktifKullanici == null) return false;
                if (DateTime.Now - sonIslem > Sure)
                {
                    Cikis();
                    return false;
                }
                return true;
            }
        }

        public static void RolGerekli(params string[] roller)
        {
            if (!Aktif)
                throw new UnauthorizedAccessException(
                    "Oturum süresi doldu, lütfen tekrar giriş yapın.");
            if (Array.IndexOf(roller, AktifKullanici.Rol) < 0)
                throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
            sonIslem = DateTime.Now;
        }
    }
}