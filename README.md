<div align="center">

<a href="README.md"><img src="docs/badges/lang-tr-active.svg?v=2" alt="Türkçe (Aktif)" height="34" /></a>&nbsp;&nbsp;<a href="README.en.md"><img src="docs/badges/lang-en-inactive.svg?v=2" alt="Read in English" height="34" /></a>

<br /><br />

<img src="docs/logo.png?v=2" alt="HP Battery Charge Limiter Logo" width="118" height="118" />

# ⚡ HP Battery Charge Limiter

**HP Victus, OMEN ve Pavilion** dizüstü bilgisayarlar için BIOS'ta bulunmayan batarya şarj sınırlandırma özelliğini donanımsal olarak etkinleştiren, **Windows (DirectX WPF GPU)** ve **Linux (Rust OpenGL/egui GPU)** destekli, kurulum sihirbazı gerektirmeyen modern masaüstü kontrol merkezi.

<p align="center">
  <a href="../../releases/latest"><img src="docs/badges/release-tr.svg?v=3" alt="İndir v2.5.0" height="30" /></a>&nbsp;
  <a href="#-linux-kurulumu-ve-kullanımı-native-openglegui-gpu"><img src="docs/badges/platform-tr.svg?v=1" alt="Windows & Linux (x64)" height="30" /></a>&nbsp;
  <a href="https://github.com/kralonur/hp-charge-limit-rs"><img src="docs/badges/engine-tr.svg?v=2" alt="Çekirdek Motor" height="30" /></a>&nbsp;
  <a href="#-öne-çıkan-özellikler"><img src="docs/badges/ui-tr.svg?v=2" alt="GPU Hızlandırmalı Arayüz" height="30" /></a>&nbsp;
  <a href="#-yapay-zeka-katkısı--geliştirme-süreci"><img src="docs/badges/ai.svg?v=2" alt="Google Gemini Argon" height="30" /></a>&nbsp;
  <a href="#-lisans-ve-yasal-bildirimler"><img src="docs/badges/license-tr.svg?v=2" alt="Lisans" height="30" /></a>
</p>

[**📥 İndir (Releases)**](../../releases/latest) &nbsp;·&nbsp; [**🪟 Windows Kullanımı**](#-windows-kurulumu-ve-kullanımı-native-directx-wpf) &nbsp;·&nbsp; [**🐧 Linux Kullanımı**](#-linux-kurulumu-ve-kullanımı-native-openglegui-gpu) &nbsp;·&nbsp; [**❓ Sıkça Sorulan Sorular**](#-sıkça-sorulan-sorular-sss) &nbsp;·&nbsp; [**📦 Sürüm Geçmişi**](docs/CHANGELOG.md)

</div>

---

## 🪟 Windows Kurulumu ve Kullanımı (Native DirectX WPF)

### 1. Kurulum Sihirbazı (`.msi` / Setup) Gerekmez!
Windows sürümü **tek bir taşınabilir (`.exe`) dosyadan** (`HPChargeLimiter.exe` — yalnızca **`1.57 MB`**) oluşur. Bilgisayarınıza Python, Rust veya harici bir sürücü paketi kurmanıza **gerek yoktur**. Arka plan Rust servisi (`hp-charge-limit-rs.exe`), doğrudan bu `.exe` dosyasının içine gömülüdür.

### 2. Adım Adım Kullanım (30 Saniye)
1. Sağ taraftaki [**Releases (Sürümler)**](../../releases/latest) bölümünden **`HPChargeLimiter.exe`** dosyasını indirin ve çift tıklayarak açın.
2. Sağ taraftaki **Şarj Limiti ve Eşik Ayarları** bölümünden kullanım senaryonuza uygun bir profil seçin (veya kaydırma çubuklarıyla kendi yüzdelerinizi belirleyin):
   * **%60 Masaüstü (`%60 Durdur / %55 Başlat`):** Bilgisayarı sürekli fişe takılı kullanıyorsanız batarya ömrünü maksimum düzeyde korur.
   * **%80 Dengeli — *Önerilen* (`%80 Durdur / %75 Başlat`):** Günlük kullanım ve uzun batarya sağlığı için ideal denge.
   * **%90 Seyahat (`%90 Durdur / %85 Başlat`):** Dışarı çıkmadan önce yüksek kapasite sağlar, %100 voltaj stresinden kaçınır.
3. **"💾 Ayarları Kaydet ve Servise Uygula"** butonuna basın *(Windows Yönetici onayı isteyecektir, Evet deyin)*.
4. **Hepsi bu kadar!** Sağ üstteki durum rozeti **`Servis: ÇALIŞIYOR`** olarak yeşile dönecektir.

### 3. Programı Kapatırsam veya `.exe` Dosyasını Silersem Ne Olur?
> [!IMPORTANT]
> **"Ayarları Kaydet ve Servise Uygula"** butonuna bir kez bastığınızda, uygulama kendi içindeki çekirdek motoru otomatik olarak **`C:\Program Files\hp-charge-limit-rs\hp-charge-limit-rs.exe`** konumuna kopyalar ve resmi bir **Windows Hizmeti (`LocalSystem`)** olarak kaydeder.
>
> * **Arayüzü (`HPChargeLimiter.exe`) tamamen kapatsanız bile** arka plan servisi çalışmaya devam eder.
> * İndirdiğiniz `HPChargeLimiter.exe` dosyasını **silseniz bile**, bilgisayarınızı her açtığınızda son kaydettiğiniz şarj limiti otomatik olarak uygulanır.
> * Programı ve servisi tek tıkla sıfır kalıntıyla silmek için **🖥️ Sistem & Tercihler -> 🧹 Tam Kaldır & Tüm Kalıntıları Temizle** butonunu kullanabilirsiniz.

---

## 🐧 Linux Kurulumu ve Kullanımı (Native OpenGL/egui GPU)

Linux sürümü (**`HPChargeLimiter-Linux-x86_64`**), tıpkı Windows sürümü gibi **%100 GPU hızlandırmalıdır** (**Saf Rust + `eframe` / `egui` OpenGL 144Hz/165Hz**, Wayland & X11 uyumlu) ve arka plan Rust servisini (`hp-charge-limit-rs`) kendi içinde taşır.

### 1. İndirme ve Çalıştırma
[**Releases (Sürümler)**](../../releases/latest) sayfasından **`HPChargeLimiter-Linux-x86_64`** dosyasını indirin, çalıştırma izni verin ve başlatın:

```bash
chmod +x HPChargeLimiter-Linux-x86_64
./HPChargeLimiter-Linux-x86_64
```

### 2. Tek Tıkla `acpi_call` Çekirdek Modülü Otomatik Kurulumu
Linux'ta HP anakartın Embedded Controller (`\_SB.WMID.WHCM`) yongasına komut gönderebilmek için **`acpi_call`** çekirdek (kernel) modülüne ihtiyaç vardır. **Terminalde paket aramanıza gerek yoktur:**
* Eğer sisteminizde `/proc/acpi/call` aktif değilse, uygulamanın en üstünde turuncu bir uyarı başlığı ve **"🔧 `acpi_call` Modülünü Otomatik Kur & Etkinleştir"** butonu belirir.
* Bu butona tıkladığınızda uygulama dağıtımınızı (**Ubuntu / Debian / Linux Mint / Pop!_OS**, **Arch / Manjaro / EndeavourOS**, **Fedora** veya **openSUSE**) otomatik algılar, `pkexec` penceresiyle yetki isteyerek uygun `acpi-call-dkms` paketini kurar, `modprobe acpi_call` ile anında etkinleştirir ve her açılışta otomatik yüklenmesi için `/etc/modules-load.d/hp-charge-limit-rs.conf` dosyasını oluşturur.

### 3. `systemd` Servis Yönetimi ve Tam Kaldırma
* **"💾 Ayarları Kaydet ve Servise Uygula"** butonuna bastığınızda gömülü çekirdek motor `/usr/local/bin/hp-charge-limit-rs` konumuna kopyalanır ve resmi bir **`systemd` servisi** olarak etkinleştirilir.
* Arayüzü kapatsanız bile `systemd` servisi arka planda (ve D-Bus `UPower` adaptör olaylarını dinleyerek) çalışmaya devam eder.
* **🖥️ Sistem & Tercihler** sekmesindeki **"🧹 Tam Kaldır & Tüm Kalıntıları Temizle"** butonu ile anakartı `%100 Normal Şarj (auto)` moduna döndürebilir ve `/usr/local/bin/hp-charge-limit-rs`, `/etc/systemd/system/hp-charge-limit-rs.service`, `/var/lib/hp-charge-limit-rs`, `/var/log/hp-charge-limit-rs` dahil tüm dosyaları tek tıkla temizleyebilirsiniz.

---

## ❓ Sıkça Sorulan Sorular (SSS)

* **Pilim şu an %91 seviyesinde, sınırı %80 yaptım ama neden %80'e düşmüyor (deşarj olmuyor)?**
  * Bu **tamamen normal ve olması gereken** davranıştır. Şarj kesme (`Inhibit`) modu bataryayı zorla boşaltmaz (deşarj etmez); bataryaya giden şarj akımını **`0.00 W`** olarak keser ve bilgisayarın tüm gücünü doğrudan adaptörden çeker. Zorla deşarj edip tekrar doldurmak batarya döngü ömrünü harcayacağı için piliniz mevcut seviyesinde (%91) sabit tutulur. Bilgisayarı pilde biraz kullanıp %80'in altına düşürdüğünüzde bir daha %80'i geçmeyecektir.
* **Seyahate çıkacağım, ayarlarımı bozmadan tek seferlik %100 şarj edebilir miyim?**
  * Evet! Ana ekrandaki **"⚡ Geçici %100 Şarj Modu (Bypass)"** anahtarını açmanız yeterlidir. Bataryanız %100'e kadar dolar. Bilgisayarı yeniden başlattığınızda Bypass otomatik olarak kapanır ve eski limitiniz (%80 vb.) kendiliğinden devreye girer.
* **Hangi HP modelleri destekleniyor?**
  * ACPI WMI tablosunda `\_SB.WMID.WHCM` (metod `0x2B` — Windows'ta `root\wmi:hpqBIntM`, Linux'ta `/proc/acpi/call`) arayüzünü barındıran **HP Victus 15 / 16**, **HP OMEN 16 / 17** ve güncel **HP Pavilion Gaming / Aero** modelleriyle çalışır. Uyumluluğu uygulama içindeki **🛠️ Test & Tanılama -> Tanılamayı Çalıştır** butonuyla anında görebilirsiniz.

---

## ✨ Öne Çıkan Özellikler

* **🚀 Windows & Linux'ta %100 GPU Hızlandırmalı Arayüz:** Windows'ta yerleşik **DirectX WPF (`1.57 MB`)**, Linux'ta ise **Saf Rust `eframe`/`egui` OpenGL** grafik motoruyla çalışır. **144Hz / 165Hz** ekranlarda sıfır gecikmeyle akar ve `< 80 ms` içinde açılır.
* **☀️ / 🌙 Karanlık ve Aydınlık Tema (Dark / Light Mode):** Üst çubuktaki tema butonu ile tüm arayüz anında Karanlık veya Aydınlık moda geçer.
* **🌐 Çift Dil Desteği (`TR` / `EN`):** Tek tıkla Türkçe ve İngilizce arasında anında geçiş yapar; arayüz ızgarası sabitlendiği için dil değişiminde hiçbir buton veya rozet kaymaz.
* **ⓘ Akıllı Bilgi Rozetleri:** Her ayarın yanındaki **`ⓘ`** rozetine geldiğinizde veya tıkladığınızda, karmaşık teknik jargon olmadan doğrudan o ayarın ne işe yaradığını anlatan sade açıklamalar gösterilir.
* **🧪 Canlı Şarj Akış Testi (`--watch`) & Donanım Tanılama (`diagnose`):** Bataryaya giren enerjiyi (`mWh`/`µWh` ve `Watt`) saniye saniye ölçerek donanımınızda şarjın gerçekten kesildiğini doğrular.

---

## 🏗️ Çalışma Mimarisi (Windows & Linux)

```mermaid
flowchart LR
    subgraph GUI["🖥️ Masaüstü GPU Arayüzü"]
        UI_WIN["Windows: HPChargeLimiter.exe\n(Native C# / DirectX WPF)"]
        UI_LIN["Linux: HPChargeLimiter-Linux-x86_64\n(Native Rust / OpenGL egui)"]
    end

    subgraph SVC_LAYER["⚙️ Arka Plan Servis Katmanı"]
        WIN_SVC["Windows Hizmeti (LocalSystem)\nC:\\Program Files\\hp-charge-limit-rs"]
        LIN_SVC["Linux systemd Servisi (root)\n/usr/local/bin/hp-charge-limit-rs"]
    end

    subgraph HW["🔌 HP Anakart & Batarya Donanımı"]
        ACPI["ACPI WMI (\\_SB.WMID.WHCM · Metod 0x2B)\nWindows: root\\wmi:hpqBIntM | Linux: /proc/acpi/call"]
        EC["Embedded Controller (EC)\n0x0500 (inhibit) · 0x0300 (hold) · 0x0000 (auto)"]
    end

    UI_WIN -->|"service install / Registry"| WIN_SVC
    UI_LIN -->|"pkexec service install / RuntimeFile"| LIN_SVC
    WIN_SVC -->|"WMI Çağrısı"| ACPI
    LIN_SVC -->|"acpi_call Çağrısı"| ACPI
    ACPI -->|"Donanım Komutu"| EC
```

---

## 📂 Proje Dizin Yapısı

```text
├── .github/
│   └── workflows/
│       └── release.yml                  # 🤖 Otomatik Windows (.exe) & Linux (x86_64) GitHub Release derleyicisi
│
├── src/
│   ├── v2-native-wpf/                   # 🪟 Windows Native C# / DirectX WPF Kaynak Kodları
│   │   ├── Program.cs                   # Tekil örnek (Mutex), DPI v2 ve giriş noktası
│   │   ├── MainWindow.cs                # DirectX GPU arayüzü, Dark/Light tema, 5 sekme, sistem tepsisi
│   │   ├── Backend.cs                   # Win32 P/Invoke, WMI/CLI köprüsü, Tam Temizlik motoru
│   │   ├── I18n.cs                      # Türkçe (TR) ve İngilizce (EN) dil sözlüğü
│   │   ├── app_icon.ico                 # Çoklu katmanlı (16px..256px) Fluent uygulama simgesi
│   │   ├── build.ps1                    # Tek komutla C# / WPF derleme betiği (csc.exe)
│   │   └── core/
│   │       └── hp-charge-limit-rs.exe   # Derleme sırasında .exe içine gömülen v0.4.1 Rust çekirdeği
│   │
│   └── linux-native-egui/               # 🐧 Linux Native Rust / OpenGL (eframe/egui) Kaynak Kodları
│       ├── Cargo.toml                   # eframe 0.29 (glow OpenGL, Wayland & X11) yapılandırması
│       ├── build.rs                     # Linux hp-charge-limit-rs çekirdeğini binary içine gömen betik
│       └── src/
│           ├── main.rs                  # OpenGL GPU arayüzü, Dark/Light tema, 5 sekme, acpi_call sihirbazı
│           ├── backend.rs               # sysfs/systemd köprüsü, tek tıkla acpi_call kurucu, Tam Temizlik
│           └── i18n.rs                  # Linux için Türkçe (TR) ve İngilizce (EN) dil sözlüğü
│
├── hp-charge-limit-rs-source/           # 🦀 v0.4.1 Upstream Rust Kaynak Kodları (kralonur/hp-charge-limit-rs)
│
├── docs/
│   ├── logo.png                         # Uygulama logosu
│   ├── badges/                          # Yuvarlatılmış modern SVG rozetler
│   ├── CHANGELOG.md                     # Sürüm geçmişi
│   └── README_UPSTREAM.md               # Orijinal hp-charge-limit-rs dokümantasyonu
│
├── README.md                            # 🇹🇷 Türkçe Dokümantasyon
├── README.en.md                         # 🇬🇧 İngilizce Dokümantasyon (English Documentation)
├── LICENSE-MIT                          # MIT Lisansı tam metni
├── LICENSE-APACHE                       # Apache 2.0 Lisansı tam metni
├── NOTICE                               # Apache 2.0 Bölüm 4 uyumlu telif ve değişiklik bildirimi
└── THIRD_PARTY_NOTICES.md               # Üçüncü parti bileşen ve lisans bildirimleri
```

---

## 🔗 Ana Kaynak (Upstream Source) ve Atıf

Bu projenin donanım haberleşme çekirdeği ve arka plan servisi (**daemon**), **Onur Kral ([@kralonur](https://github.com/kralonur))** ve katkıda bulunanlar tarafından geliştirilen açık kaynaklı **`hp-charge-limit-rs`** projesine dayanmaktadır:

* **Orijinal (Upstream) GitHub Deposu:** [**https://github.com/kralonur/hp-charge-limit-rs**](https://github.com/kralonur/hp-charge-limit-rs)
* **Kullanılan Çekirdek Sürümü:** `v0.4.1` (Commit: `3cdff25dd11b05797b8f56a393a58fcc3e8d3a2c`)
* **Çekirdek Kaynak Kodları:** [`hp-charge-limit-rs-source/`](hp-charge-limit-rs-source/) *(Donanım/EC güvenliği için Rust çekirdek kodları değiştirilmeden orijinal haliyle korunmuştur)*
* **Orijinal Dokümantasyon:** [`docs/README_UPSTREAM.md`](docs/README_UPSTREAM.md)

---

## 🤖 Yapay Zeka Katkısı & Geliştirme Süreci

Bu projenin Windows (**Native C# / DirectX WPF**) ve Linux (**Native Rust / OpenGL `egui`**) masaüstü grafik arayüzleri, tek tıkla `acpi_call` otomatik kurulum sihirbazı, Karanlık/Aydınlık tema motoru, çift dil (`TR`/`EN`) mimarisi, sıfır kalıntılı kaldırma sistemi (`FullSystemCleanup`) ve dokümantasyonu **Google Gemini (Argon) / Antigravity AI** ile birlikte (*AI-Assisted / Vibe-Coding* yaklaşımıyla) geliştirilmiştir:

* **Çekirdek Rust Motoru & ACPI WMI Keşfi (`v0.4.1`):** [Onur Kral (@kralonur)](https://github.com/kralonur) — [`hp-charge-limit-rs`](https://github.com/kralonur/hp-charge-limit-rs)
* **Masaüstü Kontrol Merkezi (Windows & Linux), Görsel Arayüz & Sistem Entegrasyonu (`v1.0` – `v2.5`):** **Google Gemini (Argon)** & **Antigravity AI** eşliğinde tasarlanmış, kodlanmış ve güvenlik denetiminden geçirilmiştir.

---

## 🛠️ Kaynak Koddan Derleme

### Windows (PowerShell — 2 Saniye)
Ekstra hiçbir harici SDK indirmeye gerek kalmadan Windows 10/11'in yerleşik `.NET Framework 64-bit` derleyicisiyle derlenir:
```powershell
powershell -ExecutionPolicy Bypass -File .\src\v2-native-wpf\build.ps1
```

### Linux (Cargo — OpenGL/egui Tek Binary)
```bash
# 1. Çekirdek servisi derle ve GUI içine gömülecek klasöre kopyala:
cargo build --release --manifest-path hp-charge-limit-rs-source/Cargo.toml
mkdir -p src/linux-native-egui/core
cp hp-charge-limit-rs-source/target/release/hp-charge-limit-rs src/linux-native-egui/core/

# 2. GPU hızlandırmalı Linux masaüstü arayüzünü derle:
cargo build --release --manifest-path src/linux-native-egui/Cargo.toml
```
*(Not: Depodaki [`.github/workflows/release.yml`](.github/workflows/release.yml) iş akışı sayesinde yeni bir etiket/release oluşturduğunuzda GitHub sunucuları hem Windows hem Linux derlemesini otomatik olarak üretip Releases sayfasına yükler.)*

---

## 📜 Lisans ve Yasal Bildirimler

Bu proje, ana kaynak olan [**`hp-charge-limit-rs`**](https://github.com/kralonur/hp-charge-limit-rs) ile uyumlu olarak **MIT Lisansı** veya **Apache Lisansı (Sürüm 2.0)** altında çift lisanslıdır (**`MIT OR Apache-2.0`**):

* **MIT Lisansı:** [`LICENSE-MIT`](LICENSE-MIT)
* **Apache Lisansı 2.0:** [`LICENSE-APACHE`](LICENSE-APACHE)
* **Telif ve Değişiklik Bildirimi (Apache 2.0 Bölüm 4):** [`NOTICE`](NOTICE)
* **Üçüncü Parti Açık Kaynak Bildirimleri:** [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md)
* **Sürüm Geçmişi:** [`docs/CHANGELOG.md`](docs/CHANGELOG.md)
