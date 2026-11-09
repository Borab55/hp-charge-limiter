using System;
using System.Collections.Generic;

namespace HPChargeLimiter
{
    public static class I18n
    {
        public static string CurrentLang = "tr";

        private static readonly Dictionary<string, Dictionary<string, string>> Dict =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "tr", new Dictionary<string, string>
                {
                    { "app_title", "HP Battery Charge Limiter" },
                    { "app_subtitle", "Victus / OMEN / Pavilion Akıllı Batarya ve Şarj Yöneticisi (v2.4 Native)" },
                    { "badge_admin", "🛡️ Yönetici Modu" },
                    { "badge_user", "👤 Kullanıcı Modu" },
                    { "badge_svc_running", "Servis: ÇALIŞIYOR" },
                    { "badge_svc_stopped", "Servis: DURDURULDU" },
                    { "badge_svc_checking", "Servis: Kontrol..." },
                    { "btn_refresh", "⟳ Yenile" },
                    { "tip_theme_toggle", "Karanlık ve Aydınlık tema arasında geçiş yap" },
                    { "tip_lang_toggle", "Arayüz dilini değiştir (Türkçe / English)" },

                    { "tab_status", "⚡ Genel Durum & Limit" },
                    { "tab_advanced", "⚙️ Gelişmiş Ayarlar" },
                    { "tab_tools", "🛠️ Test & Tanılama" },
                    { "tab_logs", "📜 Sistem Günlüğü" },
                    { "tab_settings", "🖥️ Sistem & Tercihler" },

                    { "card_battery_title", "🔋 Canlı Batarya Durumu" },
                    { "state_inhibited", "🔒 Şarj Sınırlandı (Limit Devrede)" },
                    { "state_charging", "⚡ Şarj Ediliyor" },
                    { "state_discharging", "🔋 Pilde Çalışıyor" },
                    { "state_unknown", "Durum Bilinmiyor" },
                    { "state_reading", "Okunuyor..." },

                    { "metric_ac", "Güç Kaynağı:" },
                    { "metric_ac_online", "Prize Takılı (AC)" },
                    { "metric_ac_offline", "Batarya (Pilde)" },
                    { "metric_power", "Şarj Gücü:" },
                    { "metric_energy", "Doluluk / Kapasite:" },
                    { "metric_voltage", "Anlık Voltaj:" },
                    { "metric_health", "Batarya Sağlığı:" },
                    { "metric_cycles", "Döngü Sayısı:" },
                    { "metric_model", "Batarya Modeli:" },
                    { "metric_bypass", "Tam Şarj (Bypass):" },
                    { "metric_bypass_active", "⚡ Aktif (%100 Dolum)" },
                    { "metric_bypass_inactive", "Kapalı (Limit Aktif)" },
                    { "metric_firmware", "Donanım Modu:" },
                    { "metric_last_off", "Son Kapanış:" },

                    { "card_limiter_title", "⚡ Şarj Limiti ve Eşik Ayarları" },
                    { "lbl_presets", "Hızlı Profil:" },
                    { "preset_60", "%60 Masaüstü" },
                    { "preset_80", "%80 Dengeli (Önerilen)" },
                    { "preset_90", "%90 Seyahat" },
                    { "slider_stop_title", "Maksimum Şarj Limiti: %{0} (Şarjı Durdur)" },
                    { "slider_stop_desc", "Batarya bu seviyeye ulaştığında şarj kesilir ve sistem doğrudan adaptör gücüyle çalışır." },
                    { "slider_start_title", "Yeniden Başlama Eşiği: %{0} (Şarja Başla)" },
                    { "slider_start_desc", "Batarya bu seviyenin altına düşene kadar tekrar şarj edilmez (sürekli doldur-boşaltı önler)." },
                    { "lbl_off_mode", "Kapanış / Uyku Davranışı (Off-Mode):" },
                    { "off_mode_hold_desc", "Hold (Önerilen): Bilgisayar kapalıyken veya uykudayken bataryanın %100'e dolmasını engeller." },
                    { "off_mode_inhibit_desc", "Inhibit: Yalnızca sistem açıkken şarjı keser; bilgisayar kapandığında fabrika şarjına döner." },
                    { "off_mode_auto_desc", "Auto: Bilgisayar kapandığında sınırlamayı kaldırır ve normal şarja izin verir." },
                    { "off_mode_keep_desc", "Keep: Bilgisayar kapanırken mevcut donanım modunu değiştirmeden korur." },
                    { "switch_bypass", "⚡ Geçici %100 Şarj Modu (Bypass)" },
                    { "btn_save", "💾 Ayarları Kaydet ve Servise Uygula" },
                    { "btn_saving", "⏳ Uygulanıyor..." },

                    { "card_adv_title", "⚙️ Gelişmiş Servis Davranışı" },
                    { "lbl_run_mode", "Çalışırken Kullanılacak Mod (Run-Mode):" },
                    { "run_mode_inhibit_desc", "Inhibit (Önerilen): Limite ulaşıldığında şarj akımını tamamen (0 W) keser." },
                    { "run_mode_hold_desc", "Hold: Açıkken de bekletme modunu kullanır (bazı modellerde ~2W çok yavaş akım bırakabilir)." },
                    { "run_mode_auto_desc", "Auto: Şarj sınırlamasını devre dışı bırakarak standart şarj uygular." },
                    { "slider_off_min_title", "Kapanışta Minimum Batarya Koruması: %{0}" },
                    { "slider_off_min_desc", "Bilgisayarı kapattığınızda şarjınız bu seviyenin altındaysa kapalıyken şarj olmasına izin verilir." },
                    { "lbl_stop_mode", "Servis Durdurulduğunda (Stop-Mode):" },
                    { "stop_mode_auto_desc", "Auto (Önerilen): Servis durdurulursa bataryayı güvenli şekilde normal (%100) şarj moduna döndürür." },
                    { "stop_mode_hold_desc", "Hold: Servis durdurulsa bile bekletme modunu korur." },
                    { "stop_mode_inhibit_desc", "Inhibit: Servis durdurulsa bile şarj kesme modunu korur." },
                    { "stop_mode_keep_desc", "Keep: Mevcut donanım durumuna dokunmaz." },
                    { "slider_interval_title", "Denetim Sıklığı: {0} saniye" },
                    { "slider_interval_desc", "Arka plan servisinin batarya yüzdesini ve adaptör durumunu kontrol etme aralığı." },
                    { "btn_reset_defaults", "↺ Varsayılan Ayarlara Dön (%80 / %75)" },
                    { "msg_defaults_loaded", "Varsayılan değerler (%80 / %75) yüklendi. Uygulamak için Kaydet butonuna basın." },

                    { "card_watch_title", "🧪 Canlı Şarj Akış Testi (--watch)" },
                    { "card_watch_desc", "Batarya enerjisini (mWh) saniye saniye izleyerek şarj akımının gerçekten kesildiğini doğrular." },
                    { "btn_start_watch", "▶ Testi Başlat" },
                    { "btn_watching", "⏳ Ölçülüyor..." },
                    { "card_manual_title", "⚡ Anlık Donanım Komutu Gönder" },
                    { "card_manual_desc", "Servis ayarlarını değiştirmeden anakart güç kontrolcüsüne anlık mod uygular:" },
                    { "btn_set_inhibit", "Inhibit (Şarjı Kes)" },
                    { "btn_set_hold", "Hold (Kapalı Koruma)" },
                    { "btn_set_auto", "Auto (Normal Şarj)" },
                    { "card_diag_title", "🩺 Sistem ve Uyumluluk Tanılaması" },
                    { "card_diag_desc", "Anakart kimliği, BIOS sürümü, batarya kapasitesi ve WMI sürücü erişimini kontrol eder." },
                    { "btn_run_diag", "🔍 Tanılamayı Çalıştır" },

                    { "lbl_log_title", "📜 Arka Plan Servis Günlüğü (daemon.log)" },
                    { "btn_clear_log", "Temizle" },
                    { "btn_open_folder", "Klasörü Aç" },
                    { "btn_copy_log", "Panoya Kopyala" },
                    { "chk_live_tail", "Canlı Akış (Live Tail)" },

                    { "card_pref_title", "🖥️ Uygulama Tercihleri" },
                    { "sw_tray", "Pencere kapatıldığında sistem tepsisine küçült" },
                    { "sw_autostart", "Windows açıldığında arayüzü tepside başlat" },
                    { "autostart_info", "İpucu: Arka plan servisi zaten Windows ile otomatik başlar. Bu seçenek yalnızca sağ alt tepsi simgesini açar." },
                    { "btn_elevate", "🛡️ Yönetici Yetkisi ile Yeniden Başlat" },
                    { "card_service_mgr_title", "⚙️ Servis Yönetimi ve Temizlik" },
                    { "btn_restart_svc", "Yeniden Başlat" },
                    { "btn_stop_svc", "Durdur" },
                    { "btn_start_svc", "Başlat" },
                    { "btn_uninstall_svc", "Servisi Kaldır (Logları Korur)" },
                    { "btn_full_cleanup", "🧹 Tam Kaldır & Tüm Kalıntıları Temizle" },
                    { "card_about_title", "ℹ️ Sistem Özeti (v2.4 Native DirectX/WPF)" },
                    { "about_text", "• Çekirdek Servis: hp-charge-limit-rs v0.4.1 (@kralonur - MIT / Apache-2.0)\n• Arayüz & Mimari: Google Gemini (Argon) / Antigravity AI ile geliştirildi\n• Modlar: Inhibit (Şarjı Kes) · Hold (Kapalı Koruma) · Auto (Normal)\n• Servis Konumu: C:\\Program Files\\hp-charge-limit-rs\\hp-charge-limit-rs.exe" },

                    // MODERN POWER-USER INFO TIPS (Clean, concise, zero bloat)
                    { "tip_presets", "Hızlı Şarj Profilleri:\n• %60 Masaüstü (%60 / %55): Sürekli fişe takılı kullanımda batarya ömrünü maksimum düzeyde korur.\n• %80 Dengeli (%80 / %75): Günlük kullanım ve batarya sağlığı için ideal denge.\n• %90 Seyahat (%90 / %85): Dışarı çıkmadan önce yüksek kapasite sağlar, %100 voltaj stresinden kaçınır." },
                    { "tip_stop", "Maksimum Şarj Limiti:\nBatarya bu yüzdeye ulaştığında şarj akımı (0 W) kesilir ve bilgisayar doğrudan adaptör gücüyle çalışır.\n\nNot: Mevcut şarjınız seçtiğiniz limitin üzerindeyse (ör. %90 iken %80 seçtiyseniz) batarya zorla boşaltılmaz; mevcut seviyesinde sabit tutulur." },
                    { "tip_start", "Yeniden Başlama Eşiği (Histerezis):\nŞarj kesildikten sonra batarya kendi kendine %1-2 düştüğünde şarjın hemen tekrar başlayıp bataryayı yormasını engeller. Şarj yalnızca bu eşiğin altına indiğinde tekrar başlar (Stop değerinden %5 düşük olması önerilir)." },
                    { "tip_off_mode", "Kapanış / Uyku Davranışı:\nHP cihazlar tamamen kapandığında standart şarj kesme (Inhibit) modunu sıfırlar.\n\n'Hold' seçildiğinde bilgisayar kapalıyken veya uykudayken de şarj sınırlaması korunmaya devam eder." },
                    { "tip_bypass", "Geçici %100 Şarj (Bypass):\nSeyahat öncesi ayarlarınızı bozmadan tek tıkla %100 tam doluma izin verir.\n\nBilgisayarı yeniden başlattığınızda otomatik olarak kapanır ve önceki şarj limitiniz tekrar devreye girer." },
                    { "tip_run_mode", "Çalışırken Kullanılacak Mod:\n• Inhibit (Önerilen): Bilgisayar açıkken limite ulaşıldığında şarjı tamamen (0 W) durdurur.\n• Hold: Alternatif bekletme modu.\n• Auto: Sınırlama yapmadan normal şarj eder." },
                    { "tip_off_min", "Düşük Batarya Koruması:\nBilgisayarı kapattığınızda şarjınız bu değerin altındaysa (ör. %25), kapanış koruması devreye girmez ve cihaz kapalıyken güvenli seviyeye kadar şarj olur." },
                    { "tip_stop_mode", "Servis Durdurulduğunda:\nArka plan servisini manuel olarak durdurduğunuzda veya kaldırdığınızda bataryanın hangi modda bırakılacağını belirler. Güvenlik için 'Auto' (normal şarj) önerilir." },
                    { "tip_interval", "Denetim Sıklığı:\nBIOS veya adaptör tak-çıkar olayları bazen şarj kilidini sıfırlayabilir. Servis bu aralıklarla durumu kontrol edip şarj limitini tazeler (30 sn idealdir)." },
                    { "tip_watch", "Canlı Şarj Akış Testi:\nBelirlenen süre boyunca bataryaya giren/çıkan gücü (Watt ve mWh) canlı ölçer. Şarj kesme modunun donanımınızda sorunsuz çalıştığını doğrulamak için kullanılır." },
                    { "tip_manual", "Anlık Donanım Komutu:\nArka plan servis ayarlarını değiştirmeden anakarta tek seferlik mod komutu gönderir. Servis çalışıyorsa bir sonraki kontrol döngüsünde kendi ayarına geri döner." },
                    { "tip_diag", "Sistem Tanılaması:\nAnakart modelini, BIOS sürümünü, batarya tasarım/mevcut kapasitesini ve HP WMI sürücü arayüzünün durumunu raporlar." },
                    { "tip_firmware", "Aktif Donanım Modu:\n• 0x0500 (inhibit): Şarj kesildi, adaptör gücü kullanılıyor\n• 0x0300 (hold): Kapanış/uyku koruma modu aktif\n• 0x0000 (auto): Normal şarj serbest" },
                    { "tip_last_off", "Son Kapanış Kaydı:\nBilgisayar en son kapatıldığında veya uykuya alındığında batarya yüzdesini ve uygulanan koruma modunu gösterir." },
                    { "tip_tray_pref", "Sistem Tepsisine Küçültme:\nPencereyi kapattığınızda uygulamayı sağ alt bildirim alanına küçültür. Uygulamadan tamamen çıksanız bile arka plan servisi çalışmaya devam eder." },
                    { "tip_autostart_pref", "Windows ile Başlatma:\nBilgisayar açıldığında kontrol arayüzünü sağ alt sistem tepsisinde hazır olarak başlatır." },

                    { "msg_success", "Başarılı" },
                    { "msg_error", "Hata" },
                    { "msg_warning", "Uyarı" },
                    { "msg_info", "Bilgi" },
                    { "msg_saved_service", "Ayarlar kaydedildi ve Windows Hizmeti güncellendi.\n\nAktif Limit: %{0} Durdur / %{1} Başlat" },
                    { "msg_confirm_clear_log", "Servis günlüğünü (daemon.log) temizlemek istiyor musunuz?" },
                    { "msg_copied", "Günlük metni panoya kopyalandı." },
                    { "msg_uninstall_confirm", "Windows hizmetini kaldırmak ve normal (%100) şarj moduna dönmek istiyor musunuz?" },
                    { "msg_full_cleanup_confirm", "Bu işlem;\n\n• Bataryayı fabrika (%100) şarj moduna döndürür,\n• Arka plan Windows Hizmetini kaldırır,\n• Program Files, ProgramData ve AppData altındaki tüm servis, log ve ayar dosyalarını tamamen siler.\n\nDevam etmek istiyor musunuz?" },
                    { "msg_full_cleanup_done", "Servis ve tüm uygulama kalıntıları sistemden tamamen temizlendi." },
                    { "toast_minimized", "HP Charge Limiter sistem tepsisinde çalışmaya devam ediyor." },
                    { "tray_open", "HP Charge Limiter'ı Göster" },
                    { "tray_bypass", "Geçici %100 Bypass Aç/Kapat" },
                    { "tray_exit", "Çıkış" }
                }
            },
            {
                "en", new Dictionary<string, string>
                {
                    { "app_title", "HP Battery Charge Limiter" },
                    { "app_subtitle", "Victus / OMEN / Pavilion Smart Battery & Charge Manager (v2.4 Native)" },
                    { "badge_admin", "🛡️ Administrator" },
                    { "badge_user", "👤 Standard User" },
                    { "badge_svc_running", "Service: RUNNING" },
                    { "badge_svc_stopped", "Service: STOPPED" },
                    { "badge_svc_checking", "Service: Checking..." },
                    { "btn_refresh", "⟳ Refresh" },
                    { "tip_theme_toggle", "Switch between Dark Mode and Light Mode" },
                    { "tip_lang_toggle", "Switch interface language (Türkçe / English)" },

                    { "tab_status", "⚡ Status & Limiter" },
                    { "tab_advanced", "⚙️ Advanced Settings" },
                    { "tab_tools", "🛠️ Tools & Diagnostics" },
                    { "tab_logs", "📜 System Logs" },
                    { "tab_settings", "🖥️ System & Preferences" },

                    { "card_battery_title", "🔋 Live Battery Status" },
                    { "state_inhibited", "🔒 Charge Limited (Inhibited)" },
                    { "state_charging", "⚡ Charging" },
                    { "state_discharging", "🔋 On Battery" },
                    { "state_unknown", "State Unknown" },
                    { "state_reading", "Reading..." },

                    { "metric_ac", "Power Source:" },
                    { "metric_ac_online", "Plugged In (AC)" },
                    { "metric_ac_offline", "On Battery" },
                    { "metric_power", "Charge Power:" },
                    { "metric_energy", "Remaining Capacity:" },
                    { "metric_voltage", "Voltage:" },
                    { "metric_health", "Battery Health:" },
                    { "metric_cycles", "Cycle Count:" },
                    { "metric_model", "Battery Model:" },
                    { "metric_bypass", "100% Bypass:" },
                    { "metric_bypass_active", "⚡ Active (100% Charge)" },
                    { "metric_bypass_inactive", "Off (Limit Active)" },
                    { "metric_firmware", "Hardware Mode:" },
                    { "metric_last_off", "Last Off-State:" },

                    { "card_limiter_title", "⚡ Charge Limit & Thresholds" },
                    { "lbl_presets", "Quick Preset:" },
                    { "preset_60", "60% Desktop" },
                    { "preset_80", "80% Balanced (Rec.)" },
                    { "preset_90", "90% Travel" },
                    { "slider_stop_title", "Max Charge Limit: %{0} (Stop Charge)" },
                    { "slider_stop_desc", "Charging stops at this percentage and the laptop runs directly on AC adapter power." },
                    { "slider_start_title", "Resume Threshold: %{0} (Start Charge)" },
                    { "slider_start_desc", "Charging will not resume until the battery drops below this level (prevents micro-cycling)." },
                    { "lbl_off_mode", "Shutdown / Sleep Behavior (Off-Mode):" },
                    { "off_mode_hold_desc", "Hold (Recommended): Prevents the battery from charging to 100% while shut down or asleep." },
                    { "off_mode_inhibit_desc", "Inhibit: Stops charging while running; reverts to normal charging when powered off." },
                    { "off_mode_auto_desc", "Auto: Restores normal charging when the laptop is turned off." },
                    { "off_mode_keep_desc", "Keep: Leaves the current hardware mode unchanged on shutdown." },
                    { "switch_bypass", "⚡ Temporary 100% Charge (Bypass)" },
                    { "btn_save", "💾 Save & Apply to Windows Service" },
                    { "btn_saving", "⏳ Applying..." },

                    { "card_adv_title", "⚙️ Advanced Service Behavior" },
                    { "lbl_run_mode", "Active Running Mode (Run-Mode):" },
                    { "run_mode_inhibit_desc", "Inhibit (Recommended): Completely cuts charging current (0 W) once the limit is reached." },
                    { "run_mode_hold_desc", "Hold: Uses hold mode while running (may allow a ~2W trickle on some boards)." },
                    { "run_mode_auto_desc", "Auto: Disables limiting and uses standard charging while running." },
                    { "slider_off_min_title", "Shutdown Low-Battery Guard: %{0}" },
                    { "slider_off_min_desc", "If battery is below this level when shutting down, normal charging is allowed while off." },
                    { "lbl_stop_mode", "When Service Stops (Stop-Mode):" },
                    { "stop_mode_auto_desc", "Auto (Recommended): Safely restores normal (100%) charging if the service is stopped." },
                    { "stop_mode_hold_desc", "Hold: Keeps hold mode active even if the service stops." },
                    { "stop_mode_inhibit_desc", "Inhibit: Keeps inhibit mode active even if the service stops." },
                    { "stop_mode_keep_desc", "Keep: Leaves the current hardware state untouched." },
                    { "slider_interval_title", "Check Interval: {0} seconds" },
                    { "slider_interval_desc", "How often the background service checks battery percentage and AC status." },
                    { "btn_reset_defaults", "↺ Reset to Defaults (80% / 75%)" },
                    { "msg_defaults_loaded", "Default thresholds (80% / 75%) loaded. Click Save to apply." },

                    { "card_watch_title", "🧪 Live Charge Flow Test (--watch)" },
                    { "card_watch_desc", "Monitors battery energy (mWh) in real time to verify that charging current is blocked." },
                    { "btn_start_watch", "▶ Start Test" },
                    { "btn_watching", "⏳ Measuring..." },
                    { "card_manual_title", "⚡ Direct Hardware Mode Override" },
                    { "card_manual_desc", "Sends an immediate mode command to the motherboard controller without changing service settings:" },
                    { "btn_set_inhibit", "Inhibit (Stop Charge)" },
                    { "btn_set_hold", "Hold (Off-Mode Guard)" },
                    { "btn_set_auto", "Auto (Normal Charge)" },
                    { "card_diag_title", "🩺 System & Compatibility Diagnostics" },
                    { "card_diag_desc", "Checks motherboard ID, BIOS version, battery capacity, and HP WMI driver access." },
                    { "btn_run_diag", "🔍 Run Diagnostics" },

                    { "lbl_log_title", "📜 Background Service Log (daemon.log)" },
                    { "btn_clear_log", "Clear" },
                    { "btn_open_folder", "Open Folder" },
                    { "btn_copy_log", "Copy to Clipboard" },
                    { "chk_live_tail", "Live Stream (Live Tail)" },

                    { "card_pref_title", "🖥️ App Preferences" },
                    { "sw_tray", "Minimize to system tray when window is closed" },
                    { "sw_autostart", "Start GUI in system tray on Windows boot" },
                    { "autostart_info", "Tip: The background service already starts automatically with Windows. This option only launches the tray icon." },
                    { "btn_elevate", "🛡️ Restart as Administrator" },
                    { "card_service_mgr_title", "⚙️ Service Management & Cleanup" },
                    { "btn_restart_svc", "Restart Service" },
                    { "btn_stop_svc", "Stop" },
                    { "btn_start_svc", "Start" },
                    { "btn_uninstall_svc", "Uninstall Service (Keep Logs)" },
                    { "btn_full_cleanup", "🧹 Full Uninstall & Clean All Leftovers" },
                    { "card_about_title", "ℹ️ System Overview (v2.4 Native DirectX/WPF)" },
                    { "about_text", "• Core Engine: hp-charge-limit-rs v0.4.1 (@kralonur - MIT / Apache-2.0)\n• UI & Architecture: Co-developed with Google Gemini (Argon) / Antigravity AI\n• Modes: Inhibit (Stop Charge) · Hold (Off Guard) · Auto (Normal)\n• Service Path: C:\\Program Files\\hp-charge-limit-rs\\hp-charge-limit-rs.exe" },

                    // MODERN POWER-USER INFO TIPS (Clean, concise, zero bloat)
                    { "tip_presets", "Quick Charge Profiles:\n• 60% Desktop (60% / 55%): Best for laptops plugged in 24/7 to minimize long-term battery wear.\n• 80% Balanced (80% / 75%): Ideal balance between daily battery runtime and cell longevity.\n• 90% Travel (90% / 85%): High capacity before travelling while avoiding peak 100% voltage stress." },
                    { "tip_stop", "Max Charge Limit:\nWhen battery reaches this percentage, charging current drops to 0 W and the laptop runs directly on AC power.\n\nNote: If your battery is already above this limit (e.g., at 90% when set to 80%), it won't force discharge—it holds steady until naturally used." },
                    { "tip_start", "Resume Threshold (Hysteresis):\nPrevents constant start/stop micro-charging whenever the battery drops by 1%. Once limited, charging only resumes after the battery drops below this threshold (5% below Stop is recommended)." },
                    { "tip_off_mode", "Shutdown / Sleep Behavior:\nHP laptops clear standard charge inhibit when powered off.\n\nSelecting 'Hold' keeps charge protection active while the laptop is shut down or sleeping." },
                    { "tip_bypass", "Temporary 100% Charge (Bypass):\nLets you charge to 100% before a trip with a single click without changing your saved thresholds.\n\nAutomatically resets back to your normal charge limit the next time Windows restarts." },
                    { "tip_run_mode", "Active Running Mode:\n• Inhibit (Recommended): Completely cuts charging (0 W) once the limit is reached while the PC is on.\n• Hold: Alternative hold state.\n• Auto: Normal unrestricted charging." },
                    { "tip_off_min", "Shutdown Low-Battery Guard:\nIf your battery is below this level when shutting down (e.g., 25%), shutdown protection is skipped so the battery can safely charge while off." },
                    { "tip_stop_mode", "When Service Stops:\nDefines which hardware mode is left on the controller if you manually stop or uninstall the service. 'Auto' (normal charging) is recommended for safety." },
                    { "tip_interval", "Check Interval:\nBIOS events or plugging/unplugging the AC adapter can occasionally reset the charge lock. The service checks and refreshes the limit at this interval (30s is ideal)." },
                    { "tip_watch", "Live Charge Flow Test:\nSamples battery power (Watts and mWh) in real time to verify that charging is physically blocked on your hardware." },
                    { "tip_manual", "Direct Hardware Override:\nSends a one-off mode command directly to the controller without changing service settings. If the service is running, it will re-apply its configured rule on the next check." },
                    { "tip_diag", "System Diagnostics:\nReports your motherboard ID, BIOS version, design vs. full battery capacity, and HP WMI driver status." },
                    { "tip_firmware", "Active Hardware Mode:\n• 0x0500 (inhibit): Charging stopped, running on AC power\n• 0x0300 (hold): Shutdown/sleep protection active\n• 0x0000 (auto): Normal charging enabled" },
                    { "tip_last_off", "Last Off-State:\nShows the battery percentage and protection mode applied during the most recent shutdown or sleep." },
                    { "tip_tray_pref", "Minimize to System Tray:\nHides the window to the notification area when closed. Even if you exit the GUI completely, the background Windows service continues enforcing your limit." },
                    { "tip_autostart_pref", "Start GUI on Windows Boot:\nLaunches the control app minimized in the system tray when you sign in to Windows." },

                    { "msg_success", "Success" },
                    { "msg_error", "Error" },
                    { "msg_warning", "Warning" },
                    { "msg_info", "Info" },
                    { "msg_saved_service", "Settings saved and Windows Service updated.\n\nActive Limit: Stop at %{0} / Resume at %{1}" },
                    { "msg_confirm_clear_log", "Do you want to clear the service log (daemon.log)?" },
                    { "msg_copied", "Log text copied to clipboard." },
                    { "msg_uninstall_confirm", "Do you want to uninstall the Windows service and restore normal (100%) charging?" },
                    { "msg_full_cleanup_confirm", "This action will:\n\n• Restore normal (100%) factory charging,\n• Remove the background Windows Service,\n• Delete all service files, logs, and settings under Program Files, ProgramData, and AppData.\n\nDo you want to proceed?" },
                    { "msg_full_cleanup_done", "Service and all application leftovers have been completely removed." },
                    { "toast_minimized", "HP Charge Limiter is running in the system tray." },
                    { "tray_open", "Show HP Charge Limiter" },
                    { "tray_bypass", "Toggle Temporary 100% Bypass" },
                    { "tray_exit", "Exit" }
                }
            }
        };

        public static string Get(string key, params object[] args)
        {
            Dictionary<string, string> langMap;
            if (!Dict.TryGetValue(CurrentLang, out langMap))
                langMap = Dict["tr"];

            string val;
            if (!langMap.TryGetValue(key, out val))
            {
                if (!Dict["tr"].TryGetValue(key, out val))
                    val = key;
            }

            if (args != null && args.Length > 0)
            {
                try { return string.Format(val, args); }
                catch { return val; }
            }
            return val;
        }
    }
}
