# Teknik Dokümantasyon

**Afet Sonrası İhtiyaç ve Gönüllü Koordinasyon Sistemi** (P13) · Beyzanur Rana Sansar · **v1.0.0**
Kurulum ve kullanım için: [README.md](../README.md)

## İçindekiler
1. Genel bakış
2. Mimari
3. Modüller ve sorumlu sınıflar
4. Veri modeli
5. Durum makinesi
6. Dış servisler (API)
7. Güvenlik ve KVKK
8. Yapılandırma ve ortam değişkenleri
9. Veritabanı kurulumu ve migration'lar
10. Test
11. Dağıtım
12. Arayüz ve erişilebilirlik
13. Bilinen kısıtlar ve gelecek planı
14. Uçtan uca doğrulama (v1.0.0)

---

## 1. Genel bakış
Uygulama, afet sonrasında ihtiyaç bildirimlerini toplar, koordinatörün bu ihtiyaçlara uygun gönüllüleri atamasını sağlar ve her ihtiyacın bildirimden teslime kadar durumunu izler. Masaüstü (Windows Forms) bir uygulamadır; veriler yerel SQL Server LocalDB veritabanında tutulur.

**Roller**
| Rol | Yapabildikleri |
|---|---|
| Afetzede | Kayıt, giriş, ihtiyaç bildirme, kendi ihtiyacına not ekleme |
| Gönüllü | Kayıt, giriş, ihtiyaçları görme |
| Koordinatör | Tüm işlemler: gönüllü ekleme, eşleştirme, durum değiştirme, iptal, silme, rapor, bildirimler |

## 2. Mimari
Projem Sunum, İş ve Veri olmak üzere üç katmandan oluşuyor. Formlarım (Sunum) doğrudan veritabanına gitmiyor; önce İş katmanındaki servisleri çağırıyor, servisler de Veri katmanı üzerinden veritabanına erişiyor (Form → Servis → Veritabanı). Bu yapıyı seçtim çünkü kod düzenli kalıyor, kurallar tek yerde duruyor, bir katman değişince diğerleri bozulmuyor ve kod test edilebiliyor.

```mermaid
flowchart LR
    subgraph Sunum["AfetKoordinasyon.Sunum (Windows Forms)"]
        G[GirisForm / KayitForm] --- F1[Form1 ana ekran]
        F1 --- E[EslestirmeForm]
        F1 --- DT[DurumTakipForm]
        F1 --- H[HaritaForm]
        F1 --- R[RaporForm]
    end
    subgraph Is["AfetKoordinasyon.Is (iş kuralları)"]
        S1[KullaniciServisi / Oturum]
        S2[IhtiyacServisi]
        S3[DurumServisi]
        S4[IhtiyacOnceliklendirmeServisi]
        S5[HaritaServisi / KoordinatServisi]
        S6[RaporServisi / BildirimServisi]
    end
    subgraph Veri["AfetKoordinasyon.Veri"]
        DB[AfetDbContext + Varliklar + Migrations]
    end
    Sunum --> Is --> Veri --> SQL[(LocalDB)]
    S4 -.HTTPS.-> GROQ[Groq API]
    S5 -.HTTPS.-> LIQ[LocationIQ API]
    H -.harita karoları.-> OSM[OpenStreetMap]
```

**Katman kuralları**
- Sunum yalnızca İş katmanını çağırır. İstisna: varlık sınıfları (ör. `Ihtiyac`) veri taşımak için kullanılır.
- Yetki kontrolü İş katmanında yapılır (`Oturum.RolGerekli("Koordinator")`). Bir butonu gizlemek tek başına güvenlik sayılmaz.
- Bir ihtiyacın durumunu **yalnızca** `DurumServisi` değiştirir.

## 3. Modüller ve sorumlu sınıflar

| # | Modül | Açıklama | Sınıflar |
|---|---|---|---|
| 1 | Kullanıcı | Kullanıcılar kayıt olup giriş yapabiliyor ve rollerine (afetzede, gönüllü, koordinatör) göre farklı ekranlar görüyor. Parolalar şifrelenerek saklanıyor; 5 hatalı denemede hesap 15 dakika kilitleniyor. | `KullaniciServisi`, `Oturum`, `GirisForm`, `KayitForm` |
| 2 | İhtiyaç bildirimi | Kullanıcı ihtiyacını tür, açıklama, miktar, konum ve kategori bilgisiyle bildiriyor. Yeni ihtiyaç ilk olarak "Bekliyor" durumunda kaydediliyor. | `IhtiyacServisi`, `Form1` |
| 3 | Eşleştirme | Koordinatör, bekleyen bir ihtiyaca aynı bölgedeki müsait bir gönüllüyü atıyor. Bir gönüllü aynı anda sadece tek bir işte görev alabiliyor; bu kural hem kodda hem veritabanında kontrol ediliyor. Yeni gönüllüler Gönüllü Yönetimi ekranından ekleniyor. | `EslesmeServisi`, `EslestirmeForm`, `GonulluServisi`, `GonulluForm` |
| 4 | Durum takibi | Bir ihtiyaç Bekliyor → Eşleşti → Yolda → Tamamlandı sırasıyla ilerliyor ve gerekirse İptal edilebiliyor. Her durum değişikliği; kim yaptı, ne zaman ve neden bilgisiyle geçmişe kaydediliyor. | `DurumServisi`, `DurumTakipForm` |
| 5 | Yapay zekâ | Yapay zekâ, ihtiyacın açıklamasına bakarak kategori ve aciliyet (1–5) önerisi veriyor; son kararı kullanıcı veriyor. Açıklamadaki telefon, e-posta ve TC numarası yapay zekâya gönderilmeden maskeleniyor. Yapay zekâ çalışmazsa yedek kurallar devreye giriyor. | `IhtiyacOnceliklendirmeServisi` |
| 6 | Harita | Haritada her bölge, oradaki en acil duruma göre renkli bir işaretle gösteriliyor (kırmızı: kritik bekleyen, turuncu: bekleyen, mavi: süreçte); gönüllüler yeşil işaretle görünüyor. Bölgelerin koordinatları LocationIQ servisinden bir kez alınıp veritabanına kaydediliyor. | `HaritaServisi`, `KoordinatServisi`, `HaritaForm` |
| 7 | Durum raporu | Rapor, karşılanma oranını ve ihtiyaç sayılarını bölgeye ve kategoriye göre gösteriyor; sonuçlar grafikle görselleştiriliyor ve CSV dosyası olarak dışa aktarılabiliyor. | `RaporServisi`, `RaporForm` |
| 8 | Bildirim | 1 saatten uzun süredir gönüllü bekleyen kritik (aciliyet 4–5) ihtiyaçlar için koordinatöre Windows bildirimi gidiyor; bildirime tıklayınca Durum Takibi açılıyor. | `BildirimServisi`, `Form1` |
| – | Kurulum | Uygulama açılırken veritabanını oluşturur/günceller; hiç koordinatör yoksa ilk kurulum ekranını açar. | `KurulumServisi`, `VeritabaniKurulumu`, `Program` |

**Başlıca servis metotları**
| Servis | Metotlar |
|---|---|
| `KullaniciServisi` | `KayitOl`, `GirisYap`, `KoordinatorVarMi`, `IlkKoordinatoruOlustur` |
| `IhtiyacServisi` | `Listele`, `BekleyenleriListele`, `Ekle`, `Guncelle` (sahibi veya koordinatör), `Sil` (koordinatör) |
| `GonulluServisi` | `Listele`, `Ekle` (koordinatör; telefon `05` + 9 hane, konum zorunlu) |
| `EslesmeServisi` | `BekleyenIhtiyaclariGetir`, `UygunGonulluleriGetir`, `Eslestir`, `Tamamla` (→ `DurumServisi`) |
| `DurumServisi` | `Ata`, `YolaCikti`, `Tamamla`, `AtamayiKaldir`, `IptalEt`, `NotEkle`, `TakipListesi`, `GecmisiGetir`, `GecisGecerliMi` |
| `IhtiyacOnceliklendirmeServisi` | `DegerlendirAsync`, `KisiselVeriMaskele` |
| `HaritaServisi` / `KoordinatServisi` | `BolgeOzetleri`, `EksikKoordinatlariTamamlaAsync`, `TumKoordinatlariYenidenAlAsync` |
| `RaporServisi` | `Olustur(gunSayisi)`, `OzetMetni`, `CsvOlustur` |
| `BildirimServisi` | `UzunSuredirBekleyenKritikler(esikDakika)` |

## 4. Veri modeli
ER diyagramı: [er-diyagrami.png](../er-diyagrami.png)

| Tablo | Önemli alanlar | Not |
|---|---|---|
| `Kullanicilar` | Id, AdSoyad, Email (benzersiz), ParolaHash, Rol, HataliGiris, KilitBitis | Parola düz metin olarak saklanmaz (BCrypt) |
| `Ihtiyac` | IhtiyacId, Tur, Aciklama, Miktar, Tarih, AciliyetSkoru (1–5, boş olabilir), AiGerekce, KonumId, KategoriId, DurumId, KullaniciId, SatirSurumu | `SatirSurumu` = rowversion (eşzamanlılık kontrolü) |
| `Kategori` | Id, Ad | Gıda, Su, Barınma, Sağlık, Giyim, Hijyen |
| `Konum` | KonumId, Il, Ilce, Mahalle, Enlem, Boylam | Koordinatlar LocationIQ'dan bir kez alınır |
| `Durum` | DurumId, Ad | Bekliyor, Eşleşti, Yolda, Tamamlandı, İptal |
| `Gonullu` | GonulluId, AdSoyad, Telefon, Beceri, MusaitMi, KonumId | |
| `Eslesme` | EslesmeId, IhtiyacId, GonulluId, Tarih, Aktif, BitisTarihi | Silinmez, pasif yapılır |
| `DurumGecmisi` | DurumGecmisiId, IhtiyacId, EskiDurum, YeniDurum, Tarih, KullaniciId, Not | Sadece eklenir (denetim izi) |

**Veritabanı seviyesindeki kurallar**
- `UX_Eslesme_AktifIhtiyac`: bir ihtiyacın en fazla **bir** aktif eşleşmesi olabilir (`WHERE Aktif = 1` filtreli benzersiz indeks).
- `UX_Eslesme_AktifGonullu`: bir gönüllünün en fazla **bir** aktif eşleşmesi olabilir.
- `Kullanicilar.Email` benzersizdir.

## 5. Durum makinesi
```mermaid
stateDiagram-v2
    [*] --> Bekliyor: ihtiyaç bildirildi
    Bekliyor --> Eşleşti: gönüllü atandı
    Eşleşti --> Yolda: yola çıktı
    Eşleşti --> Tamamlandı
    Yolda --> Tamamlandı: teslim edildi
    Eşleşti --> Bekliyor: atama kaldırıldı (not zorunlu)
    Yolda --> Bekliyor: atama kaldırıldı (not zorunlu)
    Bekliyor --> İptal: not zorunlu
    Eşleşti --> İptal: not zorunlu
    Yolda --> İptal: not zorunlu
    Tamamlandı --> [*]
    İptal --> [*]
```
- İzin verilmeyen geçişler (ör. Bekliyor → Tamamlandı) reddedilir ve geçmişe yazılmaz.
- Tamamlandı, İptal ve atamanın kaldırılmasında aktif eşleşme kapatılır, gönüllü yeniden müsait olur.
- İptal ve atamayı kaldırma için 5–500 karakterlik not zorunludur.
- Her işlem tek `SaveChanges` ile kaydedilir: ya hepsi kaydedilir ya hiçbiri.

## 6. Dış servisler (API)

### 6.1 LocationIQ (koordinat bulma)
| | |
|---|---|
| Uç nokta | `GET https://eu1.locationiq.com/v1/search` |
| Kimlik doğrulama | `key` parametresi; değer `LOCATIONIQ_API_KEY` ortam değişkeninden okunur |
| Sorgu | Önce yapılandırılmış arama (`city`, `state`, `country=Türkiye`), bulunamazsa serbest metin |
| Doğrulama | Sonuç Türkiye sınırları dışındaysa (enlem 35.8–42.2, boylam 25.6–44.9) reddedilir |
| Limit | Ücretsiz plan: saniyede 2, günde 5000 istek → istekler arasında 600 ms beklenir |
| Hata yönetimi | 401/403: anahtar hatası mesajı; 429 ve sunucu hataları: artan bekleme ile en fazla 3 deneme; zaman aşımı 10 sn |
| Önbellek | Bulunan koordinat `Konum` tablosuna yazılır; aynı yer için tekrar istek atılmaz |

### 6.2 Groq (yapay zekâ önerisi)
| | |
|---|---|
| Uç nokta | `POST https://api.groq.com/openai/v1/chat/completions` (OpenAI uyumlu) |
| Kimlik doğrulama | `Authorization: Bearer <GROQ_API_KEY>` |
| Model | `openai/gpt-oss-120b` (`GROQ_MODEL` ortam değişkeniyle değiştirilebilir) |
| Parametreler | `temperature: 0`, `reasoning_effort: low`, `max_completion_tokens: 1024`, `response_format: json_object` |
| Beklenen cevap | `{"kategori": "...", "aciliyet": 1-5, "gerekce": "..."}` |
| Doğrulama | Kategori listede olmalı, aciliyet 1–5 tam sayı olmalı, gerekçe en fazla 300 karakter; geçersizse bir kez daha sorulur |
| Hata yönetimi | 401/403 anahtar hatası; 429 ve 5xx için en fazla 2 deneme; zaman aşımı 20 sn; başarısızlıkta kural tabanlı yedek |
| KVKK | Telefon, e-posta ve TC numarası gönderilmeden önce maskelenir |

### 6.3 OpenStreetMap (harita karoları)
GMap.NET ile gösterilir; karolar yerel önbellekte de tutulur (`ServerAndCache`). OSM kullanım kuralı gereği uygulamaya özel bir `User-Agent` gönderilir; haritada telif satırı gösterilir.

## 7. Güvenlik ve KVKK
- **Parolalar:** BCrypt ile özetlenir; en az 8 karakter. 5 hatalı denemede hesap 15 dakika kilitlenir; hata mesajı e-postanın kayıtlı olup olmadığını belli etmez.
- **Oturum:** 30 dakika işlem yapılmazsa sona erer.
- **Yetki:** Kritik işlemler İş katmanında `Oturum.RolGerekli("Koordinator")` ile korunur. Koordinatöre özel butonlar diğer rollerde gizlenir.
- **Varsayılan hesap yok:** İlk koordinatör, ilk çalıştırmada kurulum ekranından oluşturulur. Koordinatör varken bu yol kapanır.
- **Gizli anahtarlar:** API anahtarları kodda ve depoda yoktur; ortam değişkenlerinden okunur. Örnek betik (`ortam-degiskenleri-ornek.ps1`) yalnızca yer tutucu içerir; doldurulmuş kopyası `.gitignore`'dadır. Bağlantı dizesi parola içermez (Windows kimlik doğrulaması).
- **KVKK:** Örnek ve test verileri uydurmadır. Yapay zekâya kişisel veri maskelenerek gönderilir. CSV raporunda iletişim bilgisi yoktur.
- **Eşzamanlılık:** İki koordinatör aynı kaydı aynı anda değiştirirse ikincisi uyarı alır (rowversion).

## 8. Yapılandırma ve ortam değişkenleri
| Ayar | Yer | Varsayılan |
|---|---|---|
| Bağlantı dizesi `AfetDb` | `AfetKoordinasyon.Sunum\App.config` | `Server=(localdb)\MSSQLLocalDB;Database=AfetKoordinasyonDb;Trusted_Connection=True;` |
| Test bağlantı dizesi | `AfetKoordinasyon.Testler\App.config` | `...Database=AfetKoordinasyonTestDb...` |
| `LOCATIONIQ_API_KEY` | Windows kullanıcı ortam değişkeni | yok (isteğe bağlı) |
| `GROQ_API_KEY` | Windows kullanıcı ortam değişkeni | yok (isteğe bağlı) |
| `GROQ_MODEL` | Windows kullanıcı ortam değişkeni | `openai/gpt-oss-120b` |

## 9. Veritabanı kurulumu ve migration'lar
Uygulama açılırken `KurulumServisi.VeritabaniniHazirla()` → `VeritabaniKurulumu.Hazirla()` çalışır: uygulanmamış migration varsa `DbMigrator.Update()` ile uygular; veritabanı yoksa oluşturur ve `Seed` ile örnek verileri ekler. Eksik migration yoksa hiçbir şey yapmaz.

| Migration | Ne yaptı |
|---|---|
| IlkSema | Konum, Durum, Gönüllü, İhtiyaç, Eşleşme tabloları |
| KullaniciEklendi | Kullanıcılar tablosu |
| KategoriEklendi | Kategori tablosu ve ilişki |
| IhtiyacKullaniciEklendi | İhtiyacı bildiren kullanıcı |
| MusaitMiEkle | Gönüllü müsaitliği |
| KonumKoordinatEklendi | Enlem/Boylam |
| IhtiyacAciliyetEklendi | AciliyetSkoru, AiGerekce |
| DurumTakibiEklendi | DurumGecmisi, Eslesme.Aktif/BitisTarihi, SatirSurumu, benzersiz indeksler |
| VeriTutarliligiDuzeltme | Eski Seed kaynaklı tutarsız kayıtların düzeltilmesi |

Geliştirici yeni migration eklerken: Package Manager Console → `Add-Migration Ad -ProjectName AfetKoordinasyon.Veri`, sonra `Update-Database -ProjectName AfetKoordinasyon.Veri` (ya da uygulamayı çalıştırmak yeterli).

## 10. Test
- **AfetKoordinasyon.Testler** (MSTest): tüm testler geçiyor.
  - Birim: durum makinesi, yetki ve not kuralları, gönüllü ekleme kuralları, rapor/bildirim metinleri, KVKK maskeleme, yapay zekâ çıktı doğrulama, kural tabanlı yedek.
  - Entegrasyon (gerçek LocalDB, ayrı test veritabanı): atama → yola çıkış → tamamlama, çift atama (kod + indeks), iptal, eşzamanlılık.
- Testler dış API'lere istek atmaz.
- Arayüz (formlar) elle test edilir; final sürümde uçtan uca demo senaryosu baştan sona doğrulandı (bkz. 14. bölüm).

## 11. Dağıtım
- **Yöntem:** Release derlemesi + `bin\Release` klasörünün ZIP olarak paylaşılması (xcopy dağıtımı). Kurulum programı gerekmez.
- **Hedef makine:** Windows 10/11, .NET Framework 4.8, SQL Server Express LocalDB. API anahtarları isteğe bağlıdır.
- **Değerlendirilen alternatifler:** ClickOnce (Visual Studio'dan yayımlama, otomatik güncelleme), MSI kurulum paketi. Ders projesi için ZIP dağıtımı yeterli görüldü.
- **Olası dağıtım sorunları ve önlemleri:**
  | Sorun | Önlem |
  |---|---|
  | Hedefte LocalDB yok | Açılışta anlaşılır "Veritabanı hazırlanamadı" mesajı ve README'de kurulum adımı |
  | Veritabanı yok / eski | Açılışta otomatik migration |
  | Koordinatör hesabı yok | İlk kurulum ekranı |
  | API anahtarı yok | Uygulama çalışmaya devam eder (yedek kurallar, kayıtlı koordinatlar) |
  | Sadece exe kopyalanır | README'de tüm `Release` klasörünün paylaşılması gerektiği yazılı |

## 12. Arayüz ve erişilebilirlik
**Ortak tema:** `AfetKoordinasyon.Sunum/Tema.cs` tüm formlara aynı renkleri, yazı tipini (Segoe UI), buton ve tablo stilini uygular.
| Renk | Kullanım |
|---|---|
| Lacivert `#173B5C` | Araç çubukları, tablo başlıkları |
| Turuncu `#E8772E` | Ana işlem butonları (Giriş, Ekle, Kaydet, Eşleştir, CSV) |
| Kırmızı `#C0392B` | Geri alınamaz işlemler (Sil, İptal Et) |
| Turkuaz `#2A9D8F` | Yapay zekâ önerisi |

- Giriş, kayıt, ana ekran ve eşleştirme ekranlarında **projeye özel çizilmiş** arka planlar kullanılır (`Resources/ArkaPlanGiris.jpg`, `ArkaPlanAna.jpg`). Telifsizdirler, gerçek kişi içermezler. Önceki sürümdeki basın fotoğrafları kaldırıldı.
- Veri yoğun ekranlar (Durum Takibi, Harita, Rapor) okunabilirlik için açık zeminlidir.

**Kullanılabilirlik ve erişilebilirlik iyileştirmeleri (Hafta 14)**
| İyileştirme | Neden |
|---|---|
| Koyu zeminde beyaz, beyaz kartta koyu yazı | Metin ile arka plan arasında yüksek kontrast; okunabilirlik |
| Formlar kart düzeninde, etiket her alanın üstünde | Hangi kutuya ne yazılacağı açık |
| Enter = Giriş/Kaydet, Esc = Kapat, F5 = Yenile | Fare kullanmadan işlem yapılabilir |
| Açılışta imleç ilk alanda | Gereksiz tıklama yok |
| Alanlara `AccessibleName` | Ekran okuyucular alanı adıyla okur |
| Butonlarda ipucu (ToolTip) | Butonun ne yaptığı üzerine gelince görünür |
| Durum Takibi'nde butonlar durum makinesine göre açılır | Yapılamayacak işlem baştan gösterilmez |
| Koordinatöre özel butonlar diğer rollerde gizli | Önceden görünüp yetki hatası veriyordu |
| Anlamlı pencere başlıkları ("GirisForm" → "Giriş - …") | Görev çubuğunda ve ekran okuyucuda doğru ad |
| Boş alan, geçersiz e-posta, kısa parola için Türkçe uyarı | Hata nedeni açıkça söylenir |
| Miktar 1'den başlar, en fazla 100.000 | 0 adet bildirilemez, büyük miktarlar girilebilir |
| Teknik hata yığını yerine anlaşılır hata mesajı | Kullanıcı ne yapacağını bilir |

## 13. Bilinen kısıtlar ve gelecek planı
- Kural tabanlı yedek, ekli kelimelerde (ör. "bebeğim") anahtar kelimeyi yakalamayabilir.
- Formlar için otomatik arayüz testi yok.
- Afetzede ve gönüllü rolleri tüm ihtiyaç listesini görebiliyor (güncelleme/silme yetkisi yok); ileride sadece kendi kayıtlarını görmeleri sağlanabilir.
- Gönüllü kaydı silinemiyor/düzenlenemiyor (sadece ekleme var).
- Uygulama tek bilgisayarda (LocalDB) çalışır; birden çok koordinatörün farklı bilgisayarlardan çalışması için ortak bir SQL Server gerekir. Bağlantı dizesi değiştirilerek yapılabilir.
- **Gelecek:** Türkiye'deki tüm il ve ilçelerin listeden seçilmesi; gönüllüler için mobil uygulama.

## 14. Uçtan uca doğrulama (v1.0.0)
Final sürümden önce aşağıdaki senaryo uydurma verilerle baştan sona çalıştırıldı; tüm adımlar başarılı.

| # | Adım | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Koordinatör girişi (Enter) | Ana ekran açılır, sürüm başlıkta | ✅ |
| 2 | Gönüllü Yönetimi: hatalı telefon, sonra doğru kayıt | Uyarı; gönüllü "Müsait" olarak listelenir | ✅ |
| 3 | İhtiyaç: AI ile Öner → Ekle | Kategori ve aciliyet önerisi; ihtiyaç "Bekliyor" | ✅ |
| 4 | Eşleştirme | Yeni gönüllü uygun listede; ihtiyaç "Eşleşti" | ✅ |
| 5 | Durum Takibi: Yola Çıktı → Tamamlandı; son durumda butonlar pasif; boş notla iptal reddi; notla iptal | Geçmişte tüm geçişler, kullanıcı ve notlarla | ✅ |
| 6 | Harita | Bölge sayıları güncel | ✅ |
| 7 | Durum Raporu + CSV | Karşılanan sayısı arttı; CSV Excel'de açıldı | ✅ |
| 8 | Afetzede hesabı | Eşleştirme ve Rapor butonları görünmez, Sil pasif | ✅ |

Bu doğrulama sırasında bulunup düzeltilenler: gönüllü ekleme ekranının eksik olması, AI öneri metninin kesilmesi, pasif butonların aktif görünmesi (ayrıntı: CHANGELOG.md).
