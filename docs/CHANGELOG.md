# 📦 Sürüm Geçmişi (Changelog)

Bu projedeki tüm önemli değişiklikler [Semantic Versioning (SemVer)](https://semver.org/lang/tr/) standartlarına uygun olarak bu dosyada kayıt altına alınmaktadır.

---

## [v2.5.0] — 2026-11-10 *(Native Linux OpenGL/egui GPU Edition, 1-Click `acpi_call` Setup & CI/CD)*

### 🐧 Linux Native GPU Desteği ve Otomatik Ön Koşul Sihirbazı
- **🚀 %100 Saf Rust + OpenGL (`eframe` / `egui`) Linux Masaüstü Arayüzü (`src/linux-native-egui/`):** Linux kullanıcıları için tıpkı Windows'taki DirectX WPF sürümü gibi ekran kartı (GPU) hızlandırmalı, Wayland ve X11 uyumlu, 144Hz/165Hz akıcılığında çalışan tek dosyalı (`HPChargeLimiter-Linux-x86_64`) yerel arayüz geliştirildi.
- **🔧 Tek Tıkla `acpi_call` Çekirdek Modülü Otomatik Kurucu:** Sistemde `/proc/acpi/call` eksikse arayüzün en üstünde otomatik uyarı başlığı belirir. **"🔧 `acpi_call` Modülünü Otomatik Kur & Etkinleştir"** butonuna basıldığında kullanıcının dağıtımı (`Ubuntu/Debian/Mint/Pop!_OS`, `Arch/Manjaro/EndeavourOS`, `Fedora`, `openSUSE`) otomatik algılanır, `pkexec` ile gerekli DKMS paketi kurulur, `modprobe acpi_call` yüklenir ve `/etc/modules-load.d/hp-charge-limit-rs.conf` kalıcı hale getirilir.
- **🤖 GitHub Actions Otomatik Çapraz Platform Release Hattı (`.github/workflows/release.yml`):** Yeni bir `v*` etiketi (Release) oluşturulduğunda veya Actions sekmesinden tetiklendiğinde hem **Windows (`HPChargeLimiter.exe`)** hem de **Linux (`HPChargeLimiter-Linux-x86_64`)** sürümlerini bulutta derleyip otomatik olarak GitHub Releases sayfasına yükleyen CI/CD iş akışı eklendi.

---

## [v2.4.0] — 2026-11-10 *(Modern Fluent Squircle Icon, Crisp System Tray & GitHub Documentation)*

### 🎨 Yeni Simge ve GitHub Yayını Hazırlığı
- **🔋 Yeni Windows 11 Fluent Uygulama ve Tepsi Simgesi (`app_icon.ico`):** Görev çubuğunda (Taskbar) ve sistem tepsisinde (System Tray) köşeli/bulanık görünen eski simge kaldırıldı. Yerine `1024x1024` çözünürlükten 8x supersampling (`LANCZOS`) ile üretilmiş, çerçevesiz **Fluent Azure-Indigo Squircle** (`256px` -> `16px` çoklu katmanlı) modern batarya ve yıldırım simgesi eklendi.
- **🎯 Keskin Sistem Tepsisi (System Tray) Görüntüsü:** Çalışma zamanında GDI+ ile çizilen düşük çözünürlüklü kutu simge yerine doğrudan gömülü yüksek DPI `.ico` kaynağının yüklenmesi sağlandı.
- **📖 GitHub Rehberi ve AI Katkı Bildirimi (`README.md`):** Kurulum gerekmediğini, taşınabilir `.exe` ve arka plan servis çalışma mantığını, sıfır kalıntılı kaldırma adımlarını, Sıkça Sorulan Soruları (TR/EN) ve **Google Gemini (Argon) / Antigravity AI** ile geliştirme sürecini anlatan kapsamlı dokümantasyon hazırlandı.

---

## [v2.3.0] — 2026-11-10 *(Modern Power-User Copywriting & Streamlined UI Labels)*

### ✨ Metin, İpucu ve Etiket Sadeleştirmeleri
- **🎯 Modern Power-User Açıklamaları:** `ⓘ` bilgi rozetlerindeki uzun `"🔬 TEKNİK AÇIKLAMA"` başlıkları, WMI/Registry yolları ve karmaşık terimler kaldırıldı; tüm ipuçları (`360px` kompakt genişlikte) doğrudan ne işe yaradığını ve pratik öneriyi anlatan sade, akıcı bir dille yeniden yazıldı.
- **🧹 Gereksiz LED Renk İfadeleri Temizlendi:** Modele göre farklılık gösterebilen ve buton/açıklama metinlerini kalabalıklaştıran `White LED`, `Orange LED`, `0x0500 - Beyaz`, `0x0300 - Turuncu` gibi ifadeler kaldırılarak `Inhibit (Şarjı Kes)`, `Hold (Kapalı Koruma)`, `Auto (Normal Şarj)` şeklinde net ve modern etiketlere geçildi.

---

## [v2.2.0] — 2026-11-10 *(Complete Light Mode Contrast, Exclusive Info Badges & Multi-Line Telemetry)*

### 🛠️ Arayüz, Tema ve Telemetri Düzeltmeleri
- **☀️ Aydınlık Mod (Light Mode) Tam Kontrast Uyumu:** Aydınlık moddayken koyu gri (`#475569`) kalan seçili olmayan sekmeler, dil butonları, hızlı profil butonları (`%60`, `%80`, `%90`), mod seçiciler (`hold`/`inhibit`/`auto`/`keep`), ikincil butonlar, `Standard User` rozeti ve anahtar (toggle switch) arka planları açık tema paletine (`#CBD5E1` zemin / `#0F172A` koyu metin) bağlandı.
- **🎯 Sadece `ⓘ` Rozetine Özel Teknik Açıklama:** Teknik bilgi ipuçları (ToolTip) ve tıklama pencereleri artık ayar isimlerinin, anahtarların veya butonların üzerinde **çıkmaz**; yalnızca yuvarlak `ⓘ` bilgi rozetinin üzerine gelindiğinde veya tıklandığında açılır.
- **📝 Kesilmeyen Çok Satırlı Bilgi Kartları (Multi-Line Telemetry Grid):** Sol batarya kartındaki 10 telemetri hücresi dikey istiflenmiş (*Başlık üstte, Değer altta*) yapıya geçirildi ve `TextWrapping.Wrap` etkinleştirildi. `Plugged In (Online)`, `Inactive (Limiting)` gibi uzun metinler artık `...` ile yarıda kesilmeden tam genişlikte aşağı doğru akar.
- **🔌 Standart Kullanıcı Modunda Akıllı `EC Firmware Mode` Çözümlemesi:** Uygulama yönetici yetkisi olmadan (`Standard User`) çalıştırıldığında `root\wmi:hpqBIntM` doğrudan okunamadığı için ekranda çıkan `unreadable: SELECT * FROM hpqBIntM...` hatası giderildi; arka plan servisinin `daemon.log` kaydından son donanım geçişi okunarak `0x0500 (inhibit)`, `0x0300 (hold)` veya `0x0000 (auto)` net biçimde gösterilir.
- **📐 Üst Başlık Çubuğu (`⟳ Yenile / Refresh`) Koruması:** Üst bar `DockPanel` yerine 2 sütunlu `Grid` (`*` ve `Auto`) yapısına alınarak sağdaki `⟳ Refresh` butonunun dar pencerelerde bile kesilmesi önlendi.

---

## [v2.1.0] — 2026-11-09 *(Light/Dark Theme, Interactive Info Badges & Fixed-Grid Layout)*

### ✨ Yeni Özellikler ve Arayüz İyileştirmeleri
- **☀️ / 🌙 Aydınlık ve Karanlık Tema Geçişi (Light / Dark Mode):** Üst başlık çubuğuna eklenen tema butonu ile uygulama (ve Windows 11 DWM pencere başlık çubuğu) 1 milisaniye içinde Karanlık Mod (Obsidian) ve Aydınlık Mod (Slate/Snow) arasında geçiş yapar; seçiminiz `gui_config.json` içinde saklanır.
- **🔬 Etkileşimli Teknik Bilgi Rozetleri (`ⓘ`) ve Zengin İpuçları (Tooltips):** Her ayarın ve donanım metriğinin yanına `ⓘ` bilgi rozeti eklendi. Ayar başlığının veya `ⓘ` butonunun üzerine gelindiğinde (hover) veya tıklandığında ilgili parametrenin ACPI WMI / EC donanım seviyesindeki teknik açıklaması Türkçe/İngilizce olarak görüntülenir.
- **📐 Sabitlenmiş Izgara ve Rozet Genişlikleri (Sıfır Kayma):** `TR` ve `EN` dil butonlarındaki iç boşluk (padding) sıkışıklığı giderildi (`44x26px`). Üst çubuktaki rozetler, butonlar ve sol batarya kartındaki metrik sütunları sabit piksel genişliklerine kilitlendi; dil değiştirildiğinde hiçbir ikon veya rozet yerinden oynamaz.

---

## [v2.0.0] — 2026-11-09 *(Native DirectX / WPF GPU Edition)*

### 🚀 Mimari Devrim (22.8 MB $\rightarrow$ 1.44 MB)
- **%100 Native DirectX GPU Hızlandırma:** Python (`CustomTkinter` / CPU tabanlı `Tcl/Tk`) altyapısından tamamen çıkılarak Windows'un yerleşik **WPF (`PresentationFramework` / `MilCore` DirectX 9/11)** grafik motoruna geçildi. Pencere boyutlandırma, sekme geçişleri ve kaydırma çubukları (slider) artık monitörün doğal yenileme hızında (**144Hz / 165Hz**) sıfır gecikmeyle çalışır.
- **Ultra Küçük Tek Dosya (`.exe`):** `hp-charge-limit-rs.exe` (1.40 MB) çekirdek Rust motoru doğrudan `.NET` binary kaynağı (`ManifestResourceStream`) olarak gömüldü. Toplam dosya boyutu **22.88 MB'dan 1.44 MB'a** (%93.7 küçülme) indirildi.
- **Anında Açılış (< 80 ms):** PyInstaller'ın `%TEMP%` klasörüne paket açma gecikmesi (~1.5 sn) tamamen ortadan kalktı.
- **Windows 11 Karanlık Başlık Çubuğu (DWM):** `dwmapi.dll` (`DwmSetWindowAttribute` `DWMWA_USE_IMMERSIVE_DARK_MODE`) entegrasyonu ile pencere başlık çubuğu doğrudan Windows 11 koyu temasıyla bütünleştirildi.

### ✨ Yeni Özellikler
- **🧹 Tam Kaldır & Tüm Artık Dosyaları Temizle (`FullSystemCleanup`):** Tek tıkla anakart EC çipini fabrika ayarına (`0x0000 - auto`) döndüren, Windows Hizmetini silen, `C:\Program Files\hp-charge-limit-rs`, `C:\ProgramData\hp-charge-limit-rs`, `%APPDATA%\HPChargeLimiter` klasörlerini ve Registry (`HKLM\...\hp-charge-limit-rs-bypass` ile `HKCU\...\Run`) kayıtlarını tamamen temizleyen sıfırlama motoru eklendi.
- **⚡ Hızlı Şarj Profilleri:** Tek tıkla `%60 Masaüstü`, `%80 Dengeli (Önerilen)` ve `%90 Seyahat` eşiklerini uygulayan profil çubuğu eklendi.
- **🔌 Canlı EC Firmware & Son Kapanış Telemetrisi:** Sol batarya kartına anakartın anlık donanım kodu (`0x0500 (inhibit)` vb.) ve son kapanış kaydı (`offstate.last`) eklendi.

---

## [v1.0.0] — 2026-11-09 *(Python CustomTkinter Standalone Edition — Arşivlendi)*

- `hp-charge-limit-rs` v0.4.1 üzerine ilk karanlık temalı masaüstü GUI (`CustomTkinter` + `PyStray`) geliştirildi.
- Türkçe (`TR`) ve İngilizce (`EN`) çift dil desteği, kalıcı kullanıcı yapılandırması (`%APPDATA%\HPChargeLimiter\gui_config.json`), canlı `daemon.log` izleyicisi, `--watch` akış testi ve `diagnose` donanım tanılaması eklendi.
- Win32 `RegCreateKeyExW` / `ShellExecuteExW` ile hızlı geçici %100 Bypass anahtarı eklendi.
- Yedeklenen derleme: [`releases/v1.0.0-python-ctk/HPChargeLimiter-v1.0.0-python.exe`](../releases/v1.0.0-python-ctk/HPChargeLimiter-v1.0.0-python.exe)

---

## [v0.4.1-upstream] — 2026-11-09 *(Upstream Core Engine)*

- **Kaynak:** [https://github.com/kralonur/hp-charge-limit-rs](https://github.com/kralonur/hp-charge-limit-rs) (`commit 3cdff25dd11b05797b8f56a393a58fcc3e8d3a2c`)
- HP Victus, OMEN ve Pavilion dizüstü bilgisayarlar için `\_SB.WMID.WHCM` (`root\wmi:hpqBIntM`, metod `0x2B`) üzerinden donanımsal şarj limitleme yapan saf Rust servisi ve CLI aracı.
