pub fn tr<'a>(lang: &str, key: &'a str) -> &'a str {
    let is_en = lang.eq_ignore_ascii_case("en");
    match (is_en, key) {
        (false, "app_title") => "HP Battery Charge Limiter",
        (true, "app_title") => "HP Battery Charge Limiter",

        (false, "app_subtitle") => "Victus / OMEN / Pavilion Akıllı Batarya ve Şarj Yöneticisi (v2.5 Native GPU · Linux)",
        (true, "app_subtitle") => "Victus / OMEN / Pavilion Smart Battery & Charge Manager (v2.5 Native GPU · Linux)",

        (false, "badge_admin") => "🛡️ Root Yetkisi",
        (true, "badge_admin") => "🛡️ Root User",

        (false, "badge_user") => "👤 Kullanıcı Modu",
        (true, "badge_user") => "👤 Standard User",

        (false, "badge_svc_running") => "Servis: ÇALIŞIYOR",
        (true, "badge_svc_running") => "Service: RUNNING",

        (false, "badge_svc_stopped") => "Servis: DURDURULDU",
        (true, "badge_svc_stopped") => "Service: STOPPED",

        (false, "btn_refresh") => "⟳ Yenile",
        (true, "btn_refresh") => "⟳ Refresh",

        (false, "tip_theme_toggle") => "Karanlık ve Aydınlık tema arasında geçiş yap",
        (true, "tip_theme_toggle") => "Switch between Dark Mode and Light Mode",

        (false, "tab_status") => "⚡ Genel Durum & Limit",
        (true, "tab_status") => "⚡ Status & Limiter",

        (false, "tab_advanced") => "⚙️ Gelişmiş Ayarlar",
        (true, "tab_advanced") => "⚙️ Advanced Settings",

        (false, "tab_tools") => "🛠️ Test & Tanılama",
        (true, "tab_tools") => "🛠️ Tools & Diagnostics",

        (false, "tab_logs") => "📜 Sistem Günlüğü",
        (true, "tab_logs") => "📜 System Logs",

        (false, "tab_settings") => "🖥️ Sistem & Tercihler",
        (true, "tab_settings") => "🖥️ System & Preferences",

        // ACPI_CALL AUTO-INSTALLER BANNER
        (false, "acpi_missing_title") => "⚠️ acpi_call Çekirdek Modülü Yüklü Değil (/proc/acpi/call bulunamadı)",
        (true, "acpi_missing_title") => "⚠️ acpi_call Kernel Module Not Loaded (/proc/acpi/call missing)",

        (false, "acpi_missing_desc") => "Linux'ta HP anakart şarj kontrolcüsüne (EC) komut göndermek için 'acpi_call' çekirdek modülü gereklidir. Tek tıkla dağıtımınıza uygun paketi otomatik kurup etkinleştirebilirsiniz:",
        (true, "acpi_missing_desc") => "Linux requires the 'acpi_call' kernel module to communicate with the HP motherboard EC. Click below to automatically install, load, and enable it at boot for your distro:",

        (false, "btn_install_acpi") => "🔧 acpi_call Modülünü Otomatik Kur & Etkinleştir (Tek Tık)",
        (true, "btn_install_acpi") => "🔧 Auto-Install & Enable acpi_call Module (One-Click)",

        (false, "acpi_ready_badge") => "✅ acpi_call Çekirdek Modülü Aktif (/proc/acpi/call hazır)",
        (true, "acpi_ready_badge") => "✅ acpi_call Kernel Module Active (/proc/acpi/call ready)",

        // TAB 1: BATTERY & LIMITER
        (false, "card_battery_title") => "🔋 Canlı Batarya Durumu",
        (true, "card_battery_title") => "🔋 Live Battery Status",

        (false, "state_inhibited") => "🔒 Şarj Sınırlandı (Limit Devrede)",
        (true, "state_inhibited") => "🔒 Charge Limited (Inhibited)",

        (false, "state_charging") => "⚡ Şarj Ediliyor",
        (true, "state_charging") => "⚡ Charging",

        (false, "state_discharging") => "🔋 Pilde Çalışıyor",
        (true, "state_discharging") => "🔋 On Battery",

        (false, "state_unknown") => "Durum Bilinmiyor",
        (true, "state_unknown") => "State Unknown",

        (false, "metric_ac") => "Güç Kaynağı:",
        (true, "metric_ac") => "Power Source:",

        (false, "metric_ac_online") => "Prize Takılı (AC)",
        (true, "metric_ac_online") => "Plugged In (AC)",

        (false, "metric_ac_offline") => "Batarya (Pilde)",
        (true, "metric_ac_offline") => "On Battery",

        (false, "metric_power") => "Şarj Gücü:",
        (true, "metric_power") => "Charge Power:",

        (false, "metric_energy") => "Doluluk / Kapasite:",
        (true, "metric_energy") => "Remaining Capacity:",

        (false, "metric_voltage") => "Anlık Voltaj:",
        (true, "metric_voltage") => "Voltage:",

        (false, "metric_health") => "Batarya Sağlığı:",
        (true, "metric_health") => "Battery Health:",

        (false, "metric_cycles") => "Döngü Sayısı:",
        (true, "metric_cycles") => "Cycle Count:",

        (false, "metric_model") => "Batarya Modeli:",
        (true, "metric_model") => "Battery Model:",

        (false, "metric_bypass") => "Tam Şarj (Bypass):",
        (true, "metric_bypass") => "100% Bypass:",

        (false, "metric_bypass_active") => "⚡ Aktif (%100 Dolum)",
        (true, "metric_bypass_active") => "⚡ Active (100% Charge)",

        (false, "metric_bypass_inactive") => "Kapalı (Limit Aktif)",
        (true, "metric_bypass_inactive") => "Off (Limit Active)",

        (false, "metric_firmware") => "Donanım Modu:",
        (true, "metric_firmware") => "Hardware Mode:",

        (false, "metric_last_off") => "Son Kapanış:",
        (true, "metric_last_off") => "Last Off-State:",

        (false, "card_limiter_title") => "⚡ Şarj Limiti ve Eşik Ayarları",
        (true, "card_limiter_title") => "⚡ Charge Limit & Thresholds",

        (false, "lbl_presets") => "Hızlı Profil:",
        (true, "lbl_presets") => "Quick Preset:",

        (false, "preset_60") => "%60 Masaüstü",
        (true, "preset_60") => "60% Desktop",

        (false, "preset_80") => "%80 Dengeli (Önerilen)",
        (true, "preset_80") => "80% Balanced (Rec.)",

        (false, "preset_90") => "%90 Seyahat",
        (true, "preset_90") => "90% Travel",

        (false, "slider_stop_desc") => "Batarya bu seviyeye ulaştığında şarj kesilir ve sistem doğrudan adaptör gücüyle çalışır.",
        (true, "slider_stop_desc") => "Charging stops at this percentage and the laptop runs directly on AC adapter power.",

        (false, "slider_start_desc") => "Batarya bu seviyenin altına düşene kadar tekrar şarj edilmez (sürekli doldur-boşaltı önler).",
        (true, "slider_start_desc") => "Charging will not resume until the battery drops below this level (prevents micro-cycling).",

        (false, "lbl_off_mode") => "Kapanış / Uyku Davranışı (Off-Mode):",
        (true, "lbl_off_mode") => "Shutdown / Sleep Behavior (Off-Mode):",

        (false, "off_mode_hold_desc") => "Hold (Önerilen): Bilgisayar kapalıyken veya uykudayken bataryanın %100'e dolmasını engeller.",
        (true, "off_mode_hold_desc") => "Hold (Recommended): Prevents the battery from charging to 100% while shut down or asleep.",

        (false, "off_mode_inhibit_desc") => "Inhibit: Yalnızca sistem açıkken şarjı keser; bilgisayar kapandığında fabrika şarjına döner.",
        (true, "off_mode_inhibit_desc") => "Inhibit: Stops charging while running; reverts to normal charging when powered off.",

        (false, "off_mode_auto_desc") => "Auto: Bilgisayar kapandığında sınırlamayı kaldırır ve normal şarja izin verir.",
        (true, "off_mode_auto_desc") => "Auto: Restores normal charging when the laptop is turned off.",

        (false, "off_mode_keep_desc") => "Keep: Bilgisayar kapanırken mevcut donanım modunu değiştirmeden korur.",
        (true, "off_mode_keep_desc") => "Keep: Leaves the current hardware mode unchanged on shutdown.",

        (false, "switch_bypass") => "⚡ Geçici %100 Şarj Modu (Bypass)",
        (true, "switch_bypass") => "⚡ Temporary 100% Charge (Bypass)",

        (false, "btn_save") => "💾 Ayarları Kaydet ve systemd Servisine Uygula",
        (true, "btn_save") => "💾 Save & Apply to systemd Service",

        // TAB 2: ADVANCED
        (false, "card_adv_title") => "⚙️ Gelişmiş Servis Davranışı",
        (true, "card_adv_title") => "⚙️ Advanced Service Behavior",

        (false, "lbl_run_mode") => "Çalışırken Kullanılacak Mod (Run-Mode):",
        (true, "lbl_run_mode") => "Active Running Mode (Run-Mode):",

        (false, "run_mode_inhibit_desc") => "Inhibit (Önerilen): Limite ulaşıldığında şarj akımını tamamen (0 W) keser.",
        (true, "run_mode_inhibit_desc") => "Inhibit (Recommended): Completely cuts charging current (0 W) once the limit is reached.",

        (false, "run_mode_hold_desc") => "Hold: Açıkken de bekletme modunu kullanır (bazı modellerde ~2W çok yavaş akım bırakabilir).",
        (true, "run_mode_hold_desc") => "Hold: Uses hold mode while running (may allow a ~2W trickle on some boards).",

        (false, "run_mode_auto_desc") => "Auto: Şarj sınırlamasını devre dışı bırakarak standart şarj uygular.",
        (true, "run_mode_auto_desc") => "Auto: Disables limiting and uses standard charging while running.",

        (false, "slider_off_min_desc") => "Bilgisayarı kapattığınızda şarjınız bu seviyenin altındaysa kapalıyken şarj olmasına izin verilir.",
        (true, "slider_off_min_desc") => "If battery is below this level when shutting down, normal charging is allowed while off.",

        (false, "lbl_stop_mode") => "Servis Durdurulduğunda (Stop-Mode):",
        (true, "lbl_stop_mode") => "When Service Stops (Stop-Mode):",

        (false, "stop_mode_auto_desc") => "Auto (Önerilen): Servis durdurulursa bataryayı güvenli şekilde normal (%100) şarj moduna döndürür.",
        (true, "stop_mode_auto_desc") => "Auto (Recommended): Safely restores normal (100%) charging if the service is stopped.",

        (false, "stop_mode_hold_desc") => "Hold: Servis durdurulsa bile bekletme modunu korur.",
        (true, "stop_mode_hold_desc") => "Hold: Keeps hold mode active even if the service stops.",

        (false, "stop_mode_inhibit_desc") => "Inhibit: Servis durdurulsa bile şarj kesme modunu korur.",
        (true, "stop_mode_inhibit_desc") => "Inhibit: Keeps inhibit mode active even if the service stops.",

        (false, "stop_mode_keep_desc") => "Keep: Mevcut donanım durumuna dokunmaz.",
        (true, "stop_mode_keep_desc") => "Keep: Leaves the current hardware state untouched.",

        (false, "slider_interval_desc") => "Arka plan servisinin batarya yüzdesini ve adaptör durumunu kontrol etme aralığı.",
        (true, "slider_interval_desc") => "How often the background service checks battery percentage and AC status.",

        (false, "btn_reset_defaults") => "↺ Varsayılan Ayarlara Dön (%80 / %75)",
        (true, "btn_reset_defaults") => "↺ Reset to Defaults (80% / 75%)",

        // TAB 3: TOOLS
        (false, "card_watch_title") => "🧪 Canlı Şarj Akış Testi (--watch)",
        (true, "card_watch_title") => "🧪 Live Charge Flow Test (--watch)",

        (false, "card_watch_desc") => "Batarya enerjisini (µWh / mWh) saniye saniye izleyerek şarj akımının gerçekten kesildiğini doğrular.",
        (true, "card_watch_desc") => "Monitors battery energy (µWh / mWh) in real time to verify that charging current is blocked.",

        (false, "btn_start_watch") => "▶ Testi Başlat",
        (true, "btn_start_watch") => "▶ Start Test",

        (false, "card_manual_title") => "⚡ Anlık Donanım Komutu Gönder",
        (true, "card_manual_title") => "⚡ Direct Hardware Mode Override",

        (false, "card_manual_desc") => "Servis ayarlarını değiştirmeden anakart güç kontrolcüsüne anlık mod uygular:",
        (true, "card_manual_desc") => "Sends an immediate mode command to the motherboard controller without changing service settings:",

        (false, "btn_set_inhibit") => "Inhibit (Şarjı Kes)",
        (true, "btn_set_inhibit") => "Inhibit (Stop Charge)",

        (false, "btn_set_hold") => "Hold (Kapalı Koruma)",
        (true, "btn_set_hold") => "Hold (Off-Mode Guard)",

        (false, "btn_set_auto") => "Auto (Normal Şarj)",
        (true, "btn_set_auto") => "Auto (Normal Charge)",

        (false, "card_diag_title") => "🩺 Sistem, DMI ve acpi_call Tanılaması",
        (true, "card_diag_title") => "🩺 System, DMI & acpi_call Diagnostics",

        (false, "card_diag_desc") => "Anakart kimliği, BIOS sürümü, batarya kapasitesi ve /proc/acpi/call çekirdek modülünü kontrol eder.",
        (true, "card_diag_desc") => "Checks motherboard ID, BIOS version, battery capacity, and /proc/acpi/call kernel module status.",

        (false, "btn_run_diag") => "🔍 Tanılamayı Çalıştır",
        (true, "btn_run_diag") => "🔍 Run Diagnostics",

        // TAB 4: LOGS
        (false, "lbl_log_title") => "📜 Arka Plan Servis Günlüğü (/var/log/hp-charge-limit-rs/daemon.log)",
        (true, "lbl_log_title") => "📜 Background Service Log (/var/log/hp-charge-limit-rs/daemon.log)",

        (false, "btn_clear_log") => "Temizle",
        (true, "btn_clear_log") => "Clear",

        (false, "btn_open_folder") => "Klasörü Aç",
        (true, "btn_open_folder") => "Open Folder",

        (false, "btn_copy_log") => "Panoya Kopyala",
        (true, "btn_copy_log") => "Copy to Clipboard",

        (false, "chk_live_tail") => "Canlı Akış (Live Tail)",
        (true, "chk_live_tail") => "Live Stream (Live Tail)",

        // TAB 5: SETTINGS & CLEANUP
        (false, "card_pref_title") => "🖥️ Linux Sistem & Başlangıç Tercihleri",
        (true, "card_pref_title") => "🖥️ Linux System & Startup Preferences",

        (false, "sw_autostart") => "Oturum açıldığında arayüzü otomatik başlat (~/.config/autostart)",
        (true, "sw_autostart") => "Autostart GUI on desktop login (~/.config/autostart)",

        (false, "autostart_info") => "İpucu: systemd servisi zaten arka planda otomatik çalışır. Ayrıca aşağıdan tek tıkla acpi_call modülünü kalıcı hale getirebilirsiniz.",
        (true, "autostart_info") => "Tip: The systemd service already runs automatically in the background at boot.",

        (false, "btn_elevate") => "🛡️ Root Yetkisi (pkexec) ile Yeniden Başlat",
        (true, "btn_elevate") => "🛡️ Restart with Root Privileges (pkexec)",

        (false, "card_service_mgr_title") => "⚙️ systemd Servis Yönetimi ve Temizlik",
        (true, "card_service_mgr_title") => "⚙️ systemd Service Management & Cleanup",

        (false, "btn_restart_svc") => "Yeniden Başlat",
        (true, "btn_restart_svc") => "Restart Service",

        (false, "btn_stop_svc") => "Durdur",
        (true, "btn_stop_svc") => "Stop",

        (false, "btn_start_svc") => "Başlat",
        (true, "btn_start_svc") => "Start",

        (false, "btn_uninstall_svc") => "Servisi Kaldır (Logları Korur)",
        (true, "btn_uninstall_svc") => "Uninstall Service (Keep Logs)",

        (false, "btn_full_cleanup") => "🧹 Tam Kaldır & Tüm Kalıntıları Temizle",
        (true, "btn_full_cleanup") => "🧹 Full Uninstall & Clean All Leftovers",

        (false, "card_about_title") => "ℹ️ Sistem Özeti (v2.5 Native OpenGL/egui · Linux)",
        (true, "card_about_title") => "ℹ️ System Overview (v2.5 Native OpenGL/egui · Linux)",

        (false, "about_text") => "• Çekirdek Servis: hp-charge-limit-rs v0.4.1 (@kralonur - MIT / Apache-2.0)\n• Arayüz & Mimari: Google Gemini (Argon) / Antigravity AI ile geliştirildi\n• Grafik Motoru: %100 Saf Rust + eframe/egui (OpenGL GPU Hızlandırmalı)\n• Servis Konumu: /usr/local/bin/hp-charge-limit-rs & systemd",
        (true, "about_text") => "• Core Engine: hp-charge-limit-rs v0.4.1 (@kralonur - MIT / Apache-2.0)\n• UI & Architecture: Co-developed with Google Gemini (Argon) / Antigravity AI\n• Graphics Engine: 100% Pure Rust + eframe/egui (OpenGL GPU-Accelerated)\n• Service Path: /usr/local/bin/hp-charge-limit-rs & systemd",

        // POWER-USER INFO TOOLTIPS
        (false, "tip_presets") => "Hızlı Şarj Profilleri:\n• %60 Masaüstü (%60 / %55): Sürekli fişe takılı kullanımda batarya ömrünü maksimum düzeyde korur.\n• %80 Dengeli (%80 / %75): Günlük kullanım ve batarya sağlığı için ideal denge.\n• %90 Seyahat (%90 / %85): Dışarı çıkmadan önce yüksek kapasite sağlar.",
        (true, "tip_presets") => "Quick Charge Profiles:\n• 60% Desktop (60% / 55%): Best for laptops plugged in 24/7 to minimize long-term battery wear.\n• 80% Balanced (80% / 75%): Ideal balance between daily battery runtime and cell longevity.\n• 90% Travel (90% / 85%): High capacity before travelling while avoiding peak 100% voltage stress.",

        (false, "tip_stop") => "Maksimum Şarj Limiti:\nBatarya bu yüzdeye ulaştığında şarj akımı (0 W) kesilir ve bilgisayar doğrudan adaptör gücüyle çalışır.",
        (true, "tip_stop") => "Max Charge Limit:\nWhen battery reaches this percentage, charging current drops to 0 W and the laptop runs directly on AC power.",

        (false, "tip_start") => "Yeniden Başlama Eşiği (Histerezis):\nŞarj kesildikten sonra batarya kendi kendine %1-2 düştüğünde şarjın hemen tekrar başlayıp bataryayı yormasını engeller.",
        (true, "tip_start") => "Resume Threshold (Hysteresis):\nPrevents constant start/stop micro-charging whenever the battery drops by 1%. Once limited, charging only resumes below this threshold.",

        (false, "tip_off_mode") => "Kapanış / Uyku Davranışı:\n'Hold' seçildiğinde bilgisayar kapalıyken veya uykudayken de şarj sınırlaması korunmaya devam eder.",
        (true, "tip_off_mode") => "Shutdown / Sleep Behavior:\nSelecting 'Hold' keeps charge protection active while the laptop is shut down or sleeping.",

        (false, "tip_bypass") => "Geçici %100 Şarj (Bypass):\n/run/hp-charge-limit-rs.bypass (tmpfs) oluşturarak tek seferlik %100 tam doluma izin verir. Bilgisayar yeniden başlatıldığında otomatik sıfırlanır.",
        (true, "tip_bypass") => "Temporary 100% Charge (Bypass):\nCreates /run/hp-charge-limit-rs.bypass (tmpfs) to allow charging to 100% before a trip. Automatically resets on reboot.",

        (false, "tip_run_mode") => "Çalışırken Kullanılacak Mod:\n• Inhibit (Önerilen): Bilgisayar açıkken limite ulaşıldığında şarjı tamamen (0 W) durdurur.\n• Hold: Alternatif bekletme modu.\n• Auto: Normal şarj.",
        (true, "tip_run_mode") => "Active Running Mode:\n• Inhibit (Recommended): Completely cuts charging (0 W) once the limit is reached.\n• Hold: Alternative hold state.\n• Auto: Normal charging.",

        (false, "tip_off_min") => "Düşük Batarya Koruması:\nBilgisayarı kapattığınızda şarjınız bu değerin altındaysa kapanış koruması devreye girmez ve cihaz kapalıyken şarj olur.",
        (true, "tip_off_min") => "Shutdown Low-Battery Guard:\nIf your battery is below this level when shutting down, shutdown protection is skipped so the battery can charge.",

        (false, "tip_stop_mode") => "Servis Durdurulduğunda:\nArka plan servisini durdurduğunuzda bataryanın hangi modda bırakılacağını belirler. 'Auto' (normal şarj) önerilir.",
        (true, "tip_stop_mode") => "When Service Stops:\nDefines which hardware mode is left on the controller if you stop the service. 'Auto' is recommended.",

        (false, "tip_interval") => "Denetim Sıklığı:\nUPower D-Bus olaylarına ek olarak servisin batarya durumunu kaç saniyede bir kontrol edeceğini belirler (30 sn idealdir).",
        (true, "tip_interval") => "Check Interval:\nIn addition to instant UPower D-Bus events, the service refreshes the limit at this interval (30s is ideal).",

        (false, "tip_firmware") => "Aktif Donanım Modu:\n• 0x0500 (inhibit): Şarj kesildi, adaptör gücü kullanılıyor\n• 0x0300 (hold): Kapanış/uyku koruma modu aktif\n• 0x0000 (auto): Normal şarj serbest",
        (true, "tip_firmware") => "Active Hardware Mode:\n• 0x0500 (inhibit): Charging stopped, running on AC power\n• 0x0300 (hold): Shutdown/sleep protection active\n• 0x0000 (auto): Normal charging enabled",

        (false, "tip_last_off") => "Son Kapanış Kaydı (/var/lib/hp-charge-limit-rs/offstate.last):\nBilgisayar en son kapatıldığında veya uykuya alındığında batarya yüzdesini ve uygulanan koruma modunu gösterir.",
        (true, "tip_last_off") => "Last Off-State (/var/lib/hp-charge-limit-rs/offstate.last):\nShows the battery percentage and protection mode applied during the most recent shutdown or sleep.",

        _ => key,
    }
}
