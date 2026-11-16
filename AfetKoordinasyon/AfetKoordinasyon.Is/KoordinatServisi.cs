using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using AfetKoordinasyon.Veri;
using AfetKoordinasyon.Veri.Varliklar;

namespace AfetKoordinasyon.Is
{
    // LocationIQ'nun döndürdüğü JSON'daki bir sonuç.
    // Sadece ihtiyacımız olan iki alanı alıyoruz: lat (enlem) ve lon (boylam).
    [DataContract]
    internal class LocationIqSonuc
    {
        [DataMember(Name = "lat")] public string Lat { get; set; }
        [DataMember(Name = "lon")] public string Lon { get; set; }
    }

    // İşlemin tamamını durduran API hataları (anahtar yanlış, internet yok, günlük sınır doldu...)
    public class HaritaApiHatasi : Exception
    {
        public HaritaApiHatasi(string mesaj) : base(mesaj) { }
    }

    // Koordinat tamamlama işleminin özeti (formda kullanıcıya gösterilir)
    public class KoordinatSonucu
    {
        public int Guncellenen { get; set; }
        public int Bulunamayan { get; set; }
        public string Hata { get; set; }   // varsa, işlemi durduran hata mesajı
    }

    // Konumların il/ilçe/mahalle bilgisini LocationIQ (REST API) ile enlem/boylama çevirir.
    public class KoordinatServisi
    {
        // API anahtarı kodda DEĞİL, bu isimdeki ortam değişkeninde duruyor.
        private const string AnahtarDegiskeni = "LOCATIONIQ_API_KEY";

        // Türkiye'ye en yakın sunucu: Avrupa (eu1)
        private const string AramaAdresi = "https://eu1.locationiq.com/v1/search";

        // Zaman aşımı: 10 saniyede yanıt gelmezse istek iptal edilir
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        // Rate limit: ücretsiz planda saniyede en fazla 2 istek -> iki istek arası en az 600 ms
        private const int IstekArasiMs = 600;
        private const int MaksDeneme = 3;
        private static DateTime sonIstekZamani = DateTime.MinValue;

        private static string AnahtariAl()
        {
            var anahtar = Environment.GetEnvironmentVariable(AnahtarDegiskeni);
            if (string.IsNullOrWhiteSpace(anahtar))
                throw new HaritaApiHatasi(
                    "Harita API anahtarı bulunamadı. LOCATIONIQ_API_KEY ortam değişkenini tanımlayıp Visual Studio'yu yeniden başlatın.");
            return anahtar.Trim();
        }

        // Koordinatı henüz olmayan konumlar için API'yi çağırır ve sonucu veritabanına kaydeder.
        // Koordinat bir kez alınınca saklanır; sonraki açılışlarda API tekrar çağrılmaz.
        public async Task<KoordinatSonucu> EksikKoordinatlariTamamlaAsync()
        {
            var sonuc = new KoordinatSonucu();
            try
            {
                var anahtar = AnahtariAl();
                using (var db = new AfetDbContext())
                {
                    var eksikler = db.Konumlar
                                     .Where(k => k.Enlem == null || k.Boylam == null)
                                     .ToList();

                    foreach (var konum in eksikler)
                    {
                        var koordinat = await KonumIcinAraAsync(anahtar, konum).ConfigureAwait(false);
                        if (koordinat == null)
                        {
                            sonuc.Bulunamayan++;
                            continue;
                        }

                        konum.Enlem = koordinat[0];
                        konum.Boylam = koordinat[1];
                        db.SaveChanges();
                        sonuc.Guncellenen++;
                    }
                }
            }
            catch (HaritaApiHatasi ex)
            {
                sonuc.Hata = ex.Message;
            }
            return sonuc;
        }

        // Önce mahalle dahil tam adresle arar; bulunamazsa sadece ilçe + il ile tekrar dener.
        private async Task<double[]> KonumIcinAraAsync(string anahtar, Konum konum)
        {
            if (!string.IsNullOrWhiteSpace(konum.Mahalle))
            {
                var tamAdres = $"{konum.Mahalle}, {konum.Ilce}, {konum.Il}, Türkiye";
                var sonuc = await AraAsync(anahtar, tamAdres).ConfigureAwait(false);
                if (sonuc != null) return sonuc;
            }
            return await AraAsync(anahtar, $"{konum.Ilce}, {konum.Il}, Türkiye").ConfigureAwait(false);
        }

        // Tek bir adres için API çağrısı. Bulunamazsa null döner; ciddi hatada HaritaApiHatasi fırlatır.
        private async Task<double[]> AraAsync(string anahtar, string adres)
        {
            var url = AramaAdresi
                    + "?key=" + Uri.EscapeDataString(anahtar)
                    + "&q=" + Uri.EscapeDataString(adres)
                    + "&format=json&limit=1&countrycodes=tr&accept-language=tr";

            for (int deneme = 1; deneme <= MaksDeneme; deneme++)
            {
                await HizSiniriniBekleAsync().ConfigureAwait(false);

                HttpResponseMessage yanit;
                try
                {
                    yanit = await http.GetAsync(url).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // Zaman aşımı: birkaç kez tekrar dene, olmazsa vazgeç
                    if (deneme == MaksDeneme)
                        throw new HaritaApiHatasi("Harita servisi zamanında yanıt vermedi (zaman aşımı). İnternet bağlantınızı kontrol edin.");
                    continue;
                }
                catch (HttpRequestException)
                {
                    throw new HaritaApiHatasi("Harita servisine bağlanılamadı. İnternet bağlantınızı kontrol edin.");
                }

                using (yanit)
                {
                    var govde = await yanit.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var kod = (int)yanit.StatusCode;

                    if (kod == 200)
                        return Ayristir(govde);

                    if (kod == 404)          // "Unable to geocode": bu adres bulunamadı
                        return null;

                    if (kod == 401 || kod == 403)
                        throw new HaritaApiHatasi("Harita API anahtarı geçersiz ya da yetkisiz. LOCATIONIQ_API_KEY değerini kontrol edin.");

                    if (kod == 429)          // Rate limit aşıldı
                    {
                        if (govde.Contains("Day"))
                            throw new HaritaApiHatasi("Harita servisinin günlük istek sınırı doldu. Lütfen yarın tekrar deneyin.");
                        await Task.Delay(1000 * deneme).ConfigureAwait(false);   // biraz bekle, tekrar dene
                        continue;
                    }

                    if (kod >= 500)          // Sunucu tarafı geçici hata
                    {
                        await Task.Delay(1000 * deneme).ConfigureAwait(false);
                        continue;
                    }

                    throw new HaritaApiHatasi("Harita servisi beklenmeyen bir yanıt verdi (HTTP " + kod + ").");
                }
            }
            throw new HaritaApiHatasi("Harita servisi birkaç denemeden sonra da yanıt vermedi. Lütfen daha sonra tekrar deneyin.");
        }

        // İki istek arasında en az 600 ms olmasını sağlar (saniyede 2 istek sınırı)
        private static async Task HizSiniriniBekleAsync()
        {
            var gecen = DateTime.Now - sonIstekZamani;
            var kalan = TimeSpan.FromMilliseconds(IstekArasiMs) - gecen;
            if (kalan > TimeSpan.Zero)
                await Task.Delay(kalan).ConfigureAwait(false);
            sonIstekZamani = DateTime.Now;
        }

        // JSON yanıtını ayrıştırır: [ { "lat": "38.88", "lon": "40.49", ... } ]
        private static double[] Ayristir(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(LocationIqSonuc[]));
            LocationIqSonuc[] sonuclar;
            using (var akis = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                sonuclar = (LocationIqSonuc[])serializer.ReadObject(akis);
            }

            var ilk = sonuclar?.FirstOrDefault();
            if (ilk == null) return null;

            // API sayıları "38.88" gibi NOKTALI metin olarak gönderiyor.
            // Türkçe Windows'ta ondalık ayırıcı virgül olduğu için InvariantCulture ile okuyoruz.
            double enlem, boylam;
            if (!double.TryParse(ilk.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out enlem) ||
                !double.TryParse(ilk.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out boylam))
                return null;

            if (enlem < -90 || enlem > 90 || boylam < -180 || boylam > 180)
                return null;

            return new[] { enlem, boylam };
        }
    }
}
