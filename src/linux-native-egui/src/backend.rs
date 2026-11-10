use serde::{Deserialize, Serialize};
use std::env;
use std::fs;
use std::os::unix::fs::PermissionsExt;
use std::path::{Path, PathBuf};
use std::process::Command;

const EMBEDDED_CORE: &[u8] = include_bytes!(concat!(env!("OUT_DIR"), "/embedded_core_engine"));

pub const INSTALLED_BIN: &str = "/usr/local/bin/hp-charge-limit-rs";
pub const LOG_FILE: &str = "/var/log/hp-charge-limit-rs/daemon.log";
pub const OFFSTATE_FILE: &str = "/var/lib/hp-charge-limit-rs/offstate.last";
pub const BYPASS_FILE: &str = "/run/hp-charge-limit-rs.bypass";
pub const SERVICE_UNIT: &str = "/etc/systemd/system/hp-charge-limit-rs.service";

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct GuiConfig {
    pub language: String,
    pub theme: String,
    pub autostart_gui: bool,
    pub last_stop: u8,
    pub last_start: u8,
    pub last_interval: u64,
    pub last_run_mode: String,
    pub last_off_mode: String,
    pub last_off_min: u8,
    pub last_stop_mode: String,
}

impl Default for GuiConfig {
    fn default() -> Self {
        Self {
            language: "tr".to_string(),
            theme: "dark".to_string(),
            autostart_gui: false,
            last_stop: 80,
            last_start: 75,
            last_interval: 30,
            last_run_mode: "inhibit".to_string(),
            last_off_mode: "hold".to_string(),
            last_off_min: 40,
            last_stop_mode: "auto".to_string(),
        }
    }
}

#[derive(Clone, Debug)]
pub struct ServiceConfig {
    pub stop: u8,
    pub start: u8,
    pub interval: u64,
    pub run_mode: String,
    pub off_mode: String,
    pub off_min: u8,
    pub stop_mode: String,
}

#[derive(Clone, Debug)]
pub struct BatteryStatus {
    pub percent: u8,
    pub state: String,
    pub ac: String,
    pub energy: String,
    pub voltage: String,
    pub power: String,
    pub health: String,
    pub cycles: String,
    pub pack_model: String,
    pub bypass: bool,
    pub firmware: String,
    pub service_active: bool,
    pub last_off: String,
    pub acpi_call_present: bool,
}

impl Default for BatteryStatus {
    fn default() -> Self {
        Self {
            percent: 0,
            state: "Unknown".to_string(),
            ac: "offline".to_string(),
            energy: "--".to_string(),
            voltage: "--".to_string(),
            power: "--".to_string(),
            health: "--".to_string(),
            cycles: "--".to_string(),
            pack_model: "--".to_string(),
            bypass: false,
            firmware: "--".to_string(),
            service_active: false,
            last_off: "--".to_string(),
            acpi_call_present: Path::new("/proc/acpi/call").exists(),
        }
    }
}

pub fn config_file_path() -> PathBuf {
    let home = env::var("HOME").unwrap_or_else(|_| "/tmp".to_string());
    PathBuf::from(home)
        .join(".config")
        .join("hp-charge-limiter")
        .join("gui_config.json")
}

pub fn load_gui_config() -> GuiConfig {
    let p = config_file_path();
    if let Ok(text) = fs::read_to_string(&p) {
        if let Ok(cfg) = serde_json::from_str::<GuiConfig>(&text) {
            return cfg;
        }
    }
    GuiConfig::default()
}

pub fn save_gui_config(cfg: &GuiConfig) {
    let p = config_file_path();
    if let Some(parent) = p.parent() {
        let _ = fs::create_dir_all(parent);
    }
    if let Ok(json) = serde_json::to_string_pretty(cfg) {
        let _ = fs::write(p, json);
    }
}

pub fn is_root() -> bool {
    if let Ok(out) = Command::new("id").arg("-u").output() {
        if let Ok(s) = String::from_utf8(out.stdout) {
            return s.trim() == "0";
        }
    }
    false
}

/// Extracts the embedded `hp-charge-limit-rs` binary to `~/.cache/hp-charge-limiter/hp-charge-limit-rs`
pub fn extract_embedded_core() -> Option<PathBuf> {
    if EMBEDDED_CORE.is_empty() {
        return None;
    }
    let cache_dir = env::var("HOME")
        .map(PathBuf::from)
        .unwrap_or_else(|_| PathBuf::from("/tmp"))
        .join(".cache")
        .join("hp-charge-limiter");
    let _ = fs::create_dir_all(&cache_dir);
    let extracted = cache_dir.join("hp-charge-limit-rs");

    let needs_write = match fs::metadata(&extracted) {
        Ok(m) => m.len() != EMBEDDED_CORE.len() as u64,
        Err(_) => true,
    };
    if needs_write {
        if fs::write(&extracted, EMBEDDED_CORE).is_ok() {
            let _ = fs::set_permissions(&extracted, fs::Permissions::from_mode(0o755));
        }
    }
    Some(extracted)
}

pub fn get_core_engine_path() -> PathBuf {
    if Path::new(INSTALLED_BIN).exists() {
        return PathBuf::from(INSTALLED_BIN);
    }
    if let Some(extracted) = extract_embedded_core() {
        return extracted;
    }
    PathBuf::from("hp-charge-limit-rs")
}

pub fn run_privileged_sh(script: &str) -> Result<String, String> {
    let output = if is_root() {
        Command::new("sh").arg("-c").arg(script).output()
    } else {
        Command::new("pkexec").arg("sh").arg("-c").arg(script).output()
    };

    match output {
        Ok(out) => {
            let stdout = String::from_utf8_lossy(&out.stdout).trim().to_string();
            let stderr = String::from_utf8_lossy(&out.stderr).trim().to_string();
            if out.status.success() {
                Ok(if stdout.is_empty() { "OK".to_string() } else { stdout })
            } else {
                Err(if stderr.is_empty() { stdout } else { stderr })
            }
        }
        Err(e) => Err(format!("Failed to execute privileged command (is polkit/pkexec installed?): {}", e)),
    }
}

/// One-Click Auto-Installer & Enabler for `acpi_call` kernel module across Linux distros
pub fn install_and_enable_acpi_call() -> Result<String, String> {
    let script = r#"
set -e
if [ ! -e /proc/acpi/call ]; then
    if ! modprobe acpi_call 2>/dev/null; then
        if command -v apt-get >/dev/null 2>&1; then
            DEBIAN_FRONTEND=noninteractive apt-get update
            DEBIAN_FRONTEND=noninteractive apt-get install -y acpi-call-dkms
        elif command -v pacman >/dev/null 2>&1; then
            pacman -Sy --noconfirm --needed acpi_call-dkms || pacman -Sy --noconfirm --needed acpi_call
        elif command -v dnf >/dev/null 2>&1; then
            dnf install -y kernel-devel akmod-acpi_call || dnf install -y dkms-acpi_call || dnf install -y acpi_call
        elif command -v zypper >/dev/null 2>&1; then
            zypper --non-interactive install acpi_call-kmp-default || zypper --non-interactive install acpi_call
        elif command -v xbps-install >/dev/null 2>&1; then
            xbps-install -Sy acpi_call-dkms
        fi
        if ! modprobe acpi_call; then
            if command -v mokutil >/dev/null 2>&1 && mokutil --sb-state 2>/dev/null | grep -qi "enabled"; then
                echo "ERROR: Secure Boot is enabled in BIOS and blocked loading the unsigned acpi_call module. Please disable Secure Boot in BIOS or sign the DKMS module." >&2
                exit 1
            fi
            exit 1
        fi
    fi
fi
mkdir -p /etc/modules-load.d
echo "acpi_call" > /etc/modules-load.d/hp-charge-limit-rs.conf
echo "acpi_call module loaded and enabled at boot (/proc/acpi/call ready)."
"#;
    run_privileged_sh(script)
}

pub fn get_battery_status() -> BatteryStatus {
    let mut st = BatteryStatus::default();
    st.acpi_call_present = Path::new("/proc/acpi/call").exists();
    st.bypass = Path::new(BYPASS_FILE).exists();

    let cli = get_core_engine_path();
    if let Ok(out) = Command::new(&cli).arg("status").output() {
        let text = format!(
            "{}\n{}",
            String::from_utf8_lossy(&out.stdout),
            String::from_utf8_lossy(&out.stderr)
        );
        for raw in text.lines() {
            let line = raw.trim();
            if line.starts_with("battery:") {
                if let Some(pct_pos) = line.find('%') {
                    let num_str: String = line[8..pct_pos]
                        .chars()
                        .filter(|c| c.is_ascii_digit())
                        .collect();
                    if let Ok(v) = num_str.parse::<u8>() {
                        st.percent = v;
                    }
                }
                if line.contains("Charging") {
                    st.state = "Charging".to_string();
                } else if line.contains("Not charging") || line.contains("Full") {
                    st.state = "Not charging".to_string();
                } else if line.contains("Discharging") {
                    st.state = "Discharging".to_string();
                }
                if line.contains("AC online") {
                    st.ac = "online".to_string();
                } else if line.contains("AC offline") {
                    st.ac = "offline".to_string();
                }
            } else if line.starts_with("energy:") {
                let sub = line.trim_start_matches("energy:").trim();
                let parts: Vec<&str> = sub.split(',').collect();
                if let Some(first) = parts.first() {
                    st.energy = first.split('(').next().unwrap_or(first).trim().to_string();
                }
                for p in parts {
                    if p.contains('V') {
                        st.voltage = p.trim().to_string();
                    }
                }
            } else if line.starts_with("pack:") {
                let sub = line.trim_start_matches("pack:").trim();
                st.pack_model = sub.split(',').next().unwrap_or(sub).trim().to_string();
            } else if line.starts_with("health:") {
                let sub = line.trim_start_matches("health:").trim();
                if let Some(pct_idx) = sub.find('%') {
                    let start_idx = sub[..pct_idx]
                        .rfind(|c: char| !c.is_ascii_digit() && c != '.')
                        .map(|i| i + 1)
                        .unwrap_or(0);
                    st.health = format!("{}%", &sub[start_idx..pct_idx]);
                }
                if let Some(c_idx) = sub.find("cycles:") {
                    let after = &sub[c_idx + 7..];
                    st.cycles = after.split(',').next().unwrap_or("--").trim().to_string();
                }
                if let Some(p_idx) = sub.find("power:") {
                    let after = &sub[p_idx + 6..];
                    st.power = after.split(',').next().unwrap_or("--").trim().to_string();
                }
            } else if line.starts_with("firmware:") {
                let fw = line.trim_start_matches("firmware:").trim();
                if fw.to_lowercase().contains("unreadable") {
                    st.firmware = infer_firmware_from_log(&st.state, &st.ac);
                } else {
                    st.firmware = fw.split(',').next().unwrap_or(fw).trim().to_string();
                }
            } else if line.starts_with("service:") {
                st.service_active = line.contains("active") && !line.contains("inactive");
            } else if line.starts_with("last off:") {
                st.last_off = line.trim_start_matches("last off:").trim().to_string();
            }
        }
    }

    if st.last_off == "--" {
        if let Ok(off) = fs::read_to_string(OFFSTATE_FILE) {
            let trimmed = off.trim();
            if !trimmed.is_empty() {
                st.last_off = trimmed.to_string();
            }
        }
    }

    st
}

fn infer_firmware_from_log(state: &str, ac: &str) -> String {
    if let Ok(log) = fs::read_to_string(LOG_FILE) {
        let mut last_mode: Option<&str> = None;
        for line in log.lines() {
            if line.contains("-> inhibit") {
                last_mode = Some("0x0500 (inhibit)");
            } else if line.contains("-> hold") {
                last_mode = Some("0x0300 (hold)");
            } else if line.contains("-> auto") {
                last_mode = Some("0x0000 (auto)");
            }
        }
        if let Some(m) = last_mode {
            return m.to_string();
        }
    }
    if ac == "online" && state == "Not charging" {
        return "0x0500 (inhibit)".to_string();
    }
    if state == "Charging" {
        return "0x0000 (auto)".to_string();
    }
    "--".to_string()
}

pub fn get_service_config(gui_cfg: &GuiConfig) -> ServiceConfig {
    let mut cfg = ServiceConfig {
        stop: gui_cfg.last_stop,
        start: gui_cfg.last_start,
        interval: gui_cfg.last_interval,
        run_mode: gui_cfg.last_run_mode.clone(),
        off_mode: gui_cfg.last_off_mode.clone(),
        off_min: gui_cfg.last_off_min,
        stop_mode: gui_cfg.last_stop_mode.clone(),
    };

    if let Ok(unit) = fs::read_to_string(SERVICE_UNIT) {
        for token in unit.split_whitespace() {
            if let Some(v) = token.strip_prefix("--stop=") {
                if let Ok(n) = v.parse() { cfg.stop = n; }
            } else if let Some(v) = token.strip_prefix("--start=") {
                if let Ok(n) = v.parse() { cfg.start = n; }
            } else if let Some(v) = token.strip_prefix("--interval=") {
                if let Ok(n) = v.parse() { cfg.interval = n; }
            } else if let Some(v) = token.strip_prefix("--run-mode=") {
                cfg.run_mode = v.to_string();
            } else if let Some(v) = token.strip_prefix("--off-mode=") {
                cfg.off_mode = v.to_string();
            } else if let Some(v) = token.strip_prefix("--off-min=") {
                if let Ok(n) = v.parse() { cfg.off_min = n; }
            } else if let Some(v) = token.strip_prefix("--stop-mode=") {
                cfg.stop_mode = v.to_string();
            }
        }
    }
    cfg
}

pub fn save_and_apply_service(cfg: &ServiceConfig, gui_cfg: &mut GuiConfig) -> Result<String, String> {
    let src_bin = extract_embedded_core().unwrap_or_else(get_core_engine_path);
    let script = format!(
        "if [ '{}' != '{}' ]; then install -D -m 0755 '{}' '{}'; fi && '{}' service install --stop={} --start={} --interval={} --run-mode={} --off-mode={} --off-min={} --stop-mode={}",
        src_bin.display(),
        INSTALLED_BIN,
        src_bin.display(),
        INSTALLED_BIN,
        INSTALLED_BIN,
        cfg.stop,
        cfg.start,
        cfg.interval,
        cfg.run_mode,
        cfg.off_mode,
        cfg.off_min,
        cfg.stop_mode
    );

    let res = run_privileged_sh(&script)?;
    gui_cfg.last_stop = cfg.stop;
    gui_cfg.last_start = cfg.start;
    gui_cfg.last_interval = cfg.interval;
    gui_cfg.last_run_mode = cfg.run_mode.clone();
    gui_cfg.last_off_mode = cfg.off_mode.clone();
    gui_cfg.last_off_min = cfg.off_min;
    gui_cfg.last_stop_mode = cfg.stop_mode.clone();
    save_gui_config(gui_cfg);
    Ok(res)
}

pub fn set_bypass(enable: bool) -> Result<String, String> {
    let bin = get_core_engine_path();
    let arg = if enable { "bypass on" } else { "bypass off" };
    run_privileged_sh(&format!("'{}' {}", bin.display(), arg))
}

pub fn set_manual_mode(mode: &str) -> Result<String, String> {
    let bin = get_core_engine_path();
    run_privileged_sh(&format!("'{}' set {}", bin.display(), mode))
}

pub fn run_diagnose() -> String {
    let bin = get_core_engine_path();
    match run_privileged_sh(&format!("'{}' diagnose", bin.display())) {
        Ok(s) => s,
        Err(e) => e,
    }
}

pub fn run_watch_test(seconds: u64) -> String {
    let bin = get_core_engine_path();
    match Command::new(&bin)
        .args(["status", "--watch", &seconds.to_string()])
        .output()
    {
        Ok(out) => format!(
            "{}{}",
            String::from_utf8_lossy(&out.stdout),
            String::from_utf8_lossy(&out.stderr)
        ),
        Err(e) => e.to_string(),
    }
}

pub fn control_systemd_service(action: &str) -> Result<String, String> {
    run_privileged_sh(&format!("systemctl {} hp-charge-limit-rs.service", action))
}

pub fn uninstall_service() -> Result<String, String> {
    let bin = get_core_engine_path();
    run_privileged_sh(&format!("'{}' service uninstall", bin.display()))
}

pub fn full_system_cleanup() -> Result<String, String> {
    let bin = get_core_engine_path();
    let script = format!(
        "'{}' set auto 2>/dev/null || true; \
         '{}' service uninstall 2>/dev/null || true; \
         rm -f '{}' /run/hp-charge-limit-rs.bypass /etc/modules-load.d/hp-charge-limit-rs.conf; \
         rm -rf /var/log/hp-charge-limit-rs /var/lib/hp-charge-limit-rs",
        bin.display(),
        bin.display(),
        INSTALLED_BIN
    );
    let res = run_privileged_sh(&script)?;
    set_autostart_gui(false);
    let _ = fs::remove_file(config_file_path());
    Ok(res)
}

pub fn set_autostart_gui(enable: bool) {
    let home = env::var("HOME").unwrap_or_else(|_| "/tmp".to_string());
    let autostart_dir = PathBuf::from(&home).join(".config").join("autostart");
    let desktop_file = autostart_dir.join("hp-charge-limiter.desktop");
    if enable {
        let _ = fs::create_dir_all(&autostart_dir);
        let exe = env::current_exe()
            .unwrap_or_else(|_| PathBuf::from("hp-charge-limiter"))
            .display()
            .to_string();
        let content = format!(
            "[Desktop Entry]\nType=Application\nName=HP Battery Charge Limiter\nExec=\"{}\"\nTerminal=false\nX-GNOME-Autostart-enabled=true\n",
            exe
        );
        let _ = fs::write(desktop_file, content);
    } else {
        let _ = fs::remove_file(desktop_file);
    }
}

pub fn read_daemon_log() -> String {
    if let Ok(text) = fs::read_to_string(LOG_FILE) {
        if !text.trim().is_empty() {
            return text;
        }
    }
    if let Ok(out) = Command::new("journalctl")
        .args(["-u", "hp-charge-limit-rs.service", "-n", "100", "--no-pager"])
        .output()
    {
        let s = String::from_utf8_lossy(&out.stdout).to_string();
        if !s.trim().is_empty() {
            return s;
        }
    }
    "No log entries yet (/var/log/hp-charge-limit-rs/daemon.log)".to_string()
}

pub fn clear_daemon_log() -> Result<String, String> {
    run_privileged_sh(&format!(": > '{}'", LOG_FILE))
}

pub fn open_log_folder() {
    let _ = Command::new("xdg-open")
        .arg("/var/log/hp-charge-limit-rs")
        .spawn();
}
