# ÖRNEK: API anahtarlarını Windows KULLANICI ortam değişkeni olarak tanımlar.
# Kullanım:
#   1) Bu dosyayı "ortam-degiskenleri.ps1" adıyla kopyalayın (o ad .gitignore'da, GitHub'a gitmez).
#   2) BURAYA_... yazan yerlere kendi anahtarlarınızı yazın.
#   3) PowerShell'de çalıştırın:  powershell -ExecutionPolicy Bypass -File .\ortam-degiskenleri.ps1
#   4) Visual Studio'yu kapatıp yeniden açın.
# Bu örnek dosyaya ASLA gerçek anahtar yazmayın.

$degiskenler = @{
    "LOCATIONIQ_API_KEY" = "BURAYA_LOCATIONIQ_ANAHTARINIZ"   # harita koordinatları (isteğe bağlı)
    "GROQ_API_KEY"       = "BURAYA_GROQ_ANAHTARINIZ"         # yapay zekâ önerisi (isteğe bağlı)
}

foreach ($ad in $degiskenler.Keys) {
    $deger = $degiskenler[$ad]
    if ($deger -like "BURAYA_*") {
        Write-Host "$ad atlandı (anahtar yazılmamış)."
        continue
    }
    [Environment]::SetEnvironmentVariable($ad, $deger, "User")
    Write-Host "$ad tanımlandı."
}
Write-Host "Bitti. Visual Studio'yu yeniden başlatın."
