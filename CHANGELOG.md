# Sürüm Notları

Sürüm numaraları `ANA.ARA.YAMA` biçimindedir (Semantic Versioning). Her sürüm GitHub'da bir **Git etiketi (tag)** ile işaretlenir.

## v1.0.0 — Final sürüm (Hafta 15)
**Son entegrasyon**
- Gönüllü Yönetimi ekranı: koordinatör gönüllüleri listeler (Müsait/Görevde) ve yeni gönüllü ekler. Eşleştirme ekranından açılır.
- Gönüllü ekleme kuralları: sadece koordinatör; telefon 05 ile başlayan 11 hane; konum zorunlu. Yeni birim testleri eklendi.
- Uçtan uca demo senaryosu (gönüllü ekleme → ihtiyaç + AI önerisi → eşleştirme → yola çıkış → tamamlama → harita → rapor → roller) baştan sona doğrulandı.

**Düzeltmeler**
- Yapay zekâ öneri kutusundaki metnin kesilmesi düzeltildi (önemli bilgi ilk satırda, tamamı ipucunda).
- Temalı pasif butonların aktif gibi görünmesi düzeltildi (pasif butonlar gri).
- Ana ekran başlığında sürüm numarası gösteriliyor.

## Hafta 14 — Dokümantasyon, arayüz ve dağıtım hazırlığı
- README ve teknik dokümantasyon; ortam değişkenleri için örnek betik; `.gitignore` güncellemesi.
- Açılışta otomatik veritabanı kurulumu (migration + örnek veri), ilk kurulum ekranı (varsayılan parola yok).
- Ortak tema, kart düzenli giriş/kayıt ekranları, klavye kısayolları, erişilebilirlik iyileştirmeleri.
- Telifli ve gerçek kişi içeren fotoğraflar kaldırıldı; projeye özel arka planlar eklendi.

## Hafta 13 — Test ve hata ayıklama
- MSTest test projesi: birim ve entegrasyon testleri (ayrı test veritabanı).
- Düzeltildi: parantezli telefon numaralarının maskelenmemesi (KVKK); bildirimde "60 dakikadır" yazması.

## Hafta 12 — Raporlama ve bildirim
- Durum raporu (karşılanma oranı, bölge/kategori/aciliyet dağılımı, grafikler, CSV), harita bölge özeti, kritik bekleyen ihtiyaç bildirimi.

## Hafta 11 — Durum takibi ve koordinasyon
- Durum makinesi (Bekliyor → Eşleşti → Yolda → Tamamlandı / İptal), durum geçmişi, çift atamayı önleme (benzersiz indeks + rowversion).

## Hafta 10 — Yapay zekâ
- Groq ile kategori ve aciliyet önerisi, çıktı doğrulama, KVKK maskeleme, kural tabanlı yedek.

## Hafta 9 — Harita
- LocationIQ ile koordinat bulma, GMap.NET ile harita görünümü.

## v0.1.0 — Vize ara sürümü (Hafta 8)
- Kayıt/giriş, roller, ihtiyaç bildirimi, gönüllü eşleştirme ve durum güncelleme uçtan uca birleştirildi.

## Hafta 3–7 — Temel
- Katmanlı proje iskeleti, ER diyagramı, Entity Framework ve migration'lar, kayıt/giriş (BCrypt), kategori ve doğrulama, eşleştirme modülü.
