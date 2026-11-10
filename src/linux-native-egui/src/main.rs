mod backend;
mod i18n;

use backend::{BatteryStatus, GuiConfig, ServiceConfig};
use eframe::egui::{self, Color32, FontId, Margin, RichText, Rounding, Stroke, Vec2};
use i18n::tr;
use std::sync::mpsc::{self, Receiver, Sender};
use std::thread;
use std::time::{Duration, Instant};

enum AsyncMsg {
    BatteryUpdated(BatteryStatus),
    Toast(String, bool),
    WatchOutput(String),
    DiagOutput(String),
    LogsUpdated(String),
}

struct ChargeLimiterApp {
    gui_cfg: GuiConfig,
    svc_cfg: ServiceConfig,
    battery: BatteryStatus,
    active_tab: usize,
    is_dark: bool,
    is_root: bool,

    watch_secs: u64,
    watch_running: bool,
    watch_output: String,
    diag_output: String,
    log_text: String,
    live_tail: bool,

    status_banner: Option<(String, bool, Instant)>,
    popup_info: Option<String>,

    tx: Sender<AsyncMsg>,
    rx: Receiver<AsyncMsg>,
    last_poll: Instant,
}

impl ChargeLimiterApp {
    fn new(cc: &eframe::CreationContext<'_>) -> Self {
        let gui_cfg = backend::load_gui_config();
        let svc_cfg = backend::get_service_config(&gui_cfg);
        let is_dark = !gui_cfg.theme.eq_ignore_ascii_case("light");
        let is_root = backend::is_root();
        let battery = backend::get_battery_status();
        let log_text = backend::read_daemon_log();

        let (tx, rx) = mpsc::channel();
        let app = Self {
            gui_cfg,
            svc_cfg,
            battery,
            active_tab: 0,
            is_dark,
            is_root,
            watch_secs: 30,
            watch_running: false,
            watch_output: String::new(),
            diag_output: String::new(),
            log_text,
            live_tail: true,
            status_banner: None,
            popup_info: None,
            tx,
            rx,
            last_poll: Instant::now(),
        };
        app.apply_theme(&cc.egui_ctx);
        app
    }

    fn apply_theme(&self, ctx: &egui::Context) {
        let mut visuals = if self.is_dark {
            egui::Visuals::dark()
        } else {
            egui::Visuals::light()
        };

        if self.is_dark {
            visuals.panel_fill = Color32::from_rgb(11, 15, 25);
            visuals.window_fill = Color32::from_rgb(17, 24, 39);
            visuals.faint_bg_color = Color32::from_rgb(24, 34, 52);
            visuals.extreme_bg_color = Color32::from_rgb(9, 13, 22);
            visuals.widgets.noninteractive.bg_fill = Color32::from_rgb(24, 34, 52);
            visuals.widgets.noninteractive.fg_stroke = Stroke::new(1.0_f32, Color32::from_rgb(248, 250, 252));
            visuals.widgets.inactive.bg_fill = Color32::from_rgb(30, 41, 59);
            visuals.widgets.inactive.fg_stroke = Stroke::new(1.0_f32, Color32::from_rgb(226, 232, 240));
        } else {
            visuals.panel_fill = Color32::from_rgb(248, 250, 252);
            visuals.window_fill = Color32::from_rgb(255, 255, 255);
            visuals.faint_bg_color = Color32::from_rgb(226, 232, 240);
            visuals.extreme_bg_color = Color32::from_rgb(241, 245, 249);
            visuals.widgets.noninteractive.bg_fill = Color32::from_rgb(226, 232, 240);
            visuals.widgets.noninteractive.fg_stroke = Stroke::new(1.0_f32, Color32::from_rgb(15, 23, 42));
            visuals.widgets.inactive.bg_fill = Color32::from_rgb(203, 213, 225);
            visuals.widgets.inactive.fg_stroke = Stroke::new(1.0_f32, Color32::from_rgb(15, 23, 42));
        }

        visuals.selection.bg_fill = Color32::from_rgb(2, 132, 199);
        visuals.selection.stroke = Stroke::new(1.0_f32, Color32::WHITE);
        ctx.set_visuals(visuals);
    }

    fn trigger_refresh(&self, ctx: &egui::Context) {
        let tx = self.tx.clone();
        let ctx_clone = ctx.clone();
        let tail = self.live_tail && self.active_tab == 3;
        thread::spawn(move || {
            let st = backend::get_battery_status();
            let _ = tx.send(AsyncMsg::BatteryUpdated(st));
            if tail {
                let logs = backend::read_daemon_log();
                let _ = tx.send(AsyncMsg::LogsUpdated(logs));
            }
            ctx_clone.request_repaint();
        });
    }

    fn info_badge(&mut self, ui: &mut egui::Ui, tip_key: &str) {
        let tip_text = tr(&self.gui_cfg.language, tip_key).to_string();
        let btn = egui::Button::new(
            RichText::new("i")
                .size(11.0)
                .strong()
                .color(Color32::from_rgb(56, 189, 248)),
        )
        .min_size(Vec2::new(20.0, 20.0))
        .rounding(Rounding::same(10.0));

        let resp = ui.add(btn).on_hover_text(tip_text.clone());
        if resp.clicked() {
            self.popup_info = Some(tip_text);
        }
    }

    fn segmented_btn(
        &self,
        ui: &mut egui::Ui,
        label: &str,
        active: bool,
        min_size: Vec2,
    ) -> egui::Response {
        let (bg, fg) = if active {
            (Color32::from_rgb(2, 132, 199), Color32::WHITE)
        } else if self.is_dark {
            (Color32::from_rgb(30, 41, 59), Color32::from_rgb(226, 232, 240))
        } else {
            (Color32::from_rgb(203, 213, 225), Color32::from_rgb(15, 23, 42))
        };

        ui.add(
            egui::Button::new(RichText::new(label).size(12.5).strong().color(fg))
                .fill(bg)
                .min_size(min_size)
                .rounding(Rounding::same(6.0)),
        )
    }

    fn card_frame(&self) -> egui::Frame {
        let fill = if self.is_dark {
            Color32::from_rgb(17, 24, 39)
        } else {
            Color32::from_rgb(255, 255, 255)
        };
        let stroke_col = if self.is_dark {
            Color32::from_rgb(30, 41, 59)
        } else {
            Color32::from_rgb(203, 213, 225)
        };
        egui::Frame::none()
            .fill(fill)
            .stroke(Stroke::new(1.0_f32, stroke_col))
            .rounding(Rounding::same(10.0))
            .inner_margin(Margin::same(14.0))
    }

    fn surface_frame(&self) -> egui::Frame {
        let fill = if self.is_dark {
            Color32::from_rgb(24, 34, 52)
        } else {
            Color32::from_rgb(226, 232, 240)
        };
        egui::Frame::none()
            .fill(fill)
            .rounding(Rounding::same(8.0))
            .inner_margin(Margin::same(12.0))
    }
}

impl eframe::App for ChargeLimiterApp {
    fn update(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        ctx.request_repaint_after(Duration::from_secs(1));

        while let Ok(msg) = self.rx.try_recv() {
            match msg {
                AsyncMsg::BatteryUpdated(st) => self.battery = st,
                AsyncMsg::Toast(t, ok) => self.status_banner = Some((t, ok, Instant::now())),
                AsyncMsg::WatchOutput(s) => {
                    self.watch_output = s;
                    self.watch_running = false;
                }
                AsyncMsg::DiagOutput(s) => self.diag_output = s,
                AsyncMsg::LogsUpdated(s) => self.log_text = s,
            }
        }

        if self.last_poll.elapsed() >= Duration::from_secs(5) {
            self.last_poll = Instant::now();
            self.trigger_refresh(ctx);
        }

        let lang = self.gui_cfg.language.clone();
        let accent_text = if self.is_dark {
            Color32::from_rgb(56, 189, 248)
        } else {
            Color32::from_rgb(2, 132, 199)
        };
        let sec_text = if self.is_dark {
            Color32::from_rgb(148, 163, 184)
        } else {
            Color32::from_rgb(71, 85, 105)
        };

        egui::CentralPanel::default().show(ctx, |ui| {
            // 1. HEADER CARD
            self.surface_frame().show(ui, |ui| {
                ui.horizontal(|ui| {
                    ui.vertical(|ui| {
                        ui.label(RichText::new(tr(&lang, "app_title")).size(20.0).strong());
                        ui.label(RichText::new(tr(&lang, "app_subtitle")).size(11.5).color(sec_text));
                    });

                    ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                        if self
                            .segmented_btn(ui, tr(&lang, "btn_refresh"), false, Vec2::new(82.0, 28.0))
                            .clicked()
                        {
                            self.trigger_refresh(ctx);
                        }

                        // Service Status Badge
                        let (svc_bg, svc_txt) = if self.battery.service_active {
                            (Color32::from_rgb(5, 150, 105), tr(&lang, "badge_svc_running"))
                        } else {
                            (Color32::from_rgb(220, 38, 38), tr(&lang, "badge_svc_stopped"))
                        };
                        ui.add(
                            egui::Button::new(RichText::new(svc_txt).size(11.5).strong().color(Color32::WHITE))
                                .fill(svc_bg)
                                .min_size(Vec2::new(138.0, 28.0))
                                .rounding(Rounding::same(6.0)),
                        );

                        // Root / Standard User Badge
                        let adm_txt = if self.is_root {
                            tr(&lang, "badge_admin")
                        } else {
                            tr(&lang, "badge_user")
                        };
                        self.segmented_btn(ui, adm_txt, self.is_root, Vec2::new(124.0, 28.0));

                        // Language Toggle (TR | EN)
                        if self
                            .segmented_btn(ui, "EN", lang.eq_ignore_ascii_case("en"), Vec2::new(40.0, 28.0))
                            .clicked()
                        {
                            self.gui_cfg.language = "en".to_string();
                            backend::save_gui_config(&self.gui_cfg);
                        }
                        if self
                            .segmented_btn(ui, "TR", lang.eq_ignore_ascii_case("tr"), Vec2::new(40.0, 28.0))
                            .clicked()
                        {
                            self.gui_cfg.language = "tr".to_string();
                            backend::save_gui_config(&self.gui_cfg);
                        }

                        // Theme Toggle (☀️ / 🌙)
                        let theme_icon = if self.is_dark { "☀" } else { "🌙" };
                        if self
                            .segmented_btn(ui, theme_icon, false, Vec2::new(34.0, 28.0))
                            .on_hover_text(tr(&lang, "tip_theme_toggle"))
                            .clicked()
                        {
                            self.is_dark = !self.is_dark;
                            self.gui_cfg.theme = if self.is_dark { "dark" } else { "light" }.to_string();
                            self.apply_theme(ctx);
                            backend::save_gui_config(&self.gui_cfg);
                        }
                    });
                });
            });

            ui.add_space(6.0);

            // 2. NAVIGATION TABS (5 Equal Columns)
            let tab_keys = ["tab_status", "tab_advanced", "tab_tools", "tab_logs", "tab_settings"];
            ui.columns(5, |cols| {
                for (i, k) in tab_keys.iter().enumerate() {
                    let w = cols[i].available_width();
                    if self
                        .segmented_btn(&mut cols[i], tr(&lang, k), self.active_tab == i, Vec2::new(w, 32.0))
                        .clicked()
                    {
                        self.active_tab = i;
                        if i == 3 {
                            self.log_text = backend::read_daemon_log();
                        }
                    }
                }
            });

            ui.add_space(6.0);

            // Toast notification banner if active
            if let Some((msg, ok, ts)) = self.status_banner.clone() {
                if ts.elapsed() < Duration::from_secs(6) {
                    let col = if ok {
                        Color32::from_rgb(5, 150, 105)
                    } else {
                        Color32::from_rgb(220, 38, 38)
                    };
                    egui::Frame::none()
                        .fill(col)
                        .rounding(Rounding::same(6.0))
                        .inner_margin(Margin::symmetric(12.0, 6.0))
                        .show(ui, |ui| {
                            ui.label(RichText::new(msg).color(Color32::WHITE).strong());
                        });
                    ui.add_space(6.0);
                } else {
                    self.status_banner = None;
                }
            }

            // 3. ONE-CLICK ACPI_CALL AUTO-INSTALLER BANNER (if /proc/acpi/call is missing)
            if !self.battery.acpi_call_present {
                egui::Frame::none()
                    .fill(Color32::from_rgb(120, 53, 15))
                    .stroke(Stroke::new(1.0_f32, Color32::from_rgb(245, 158, 11)))
                    .rounding(Rounding::same(8.0))
                    .inner_margin(Margin::same(10.0))
                    .show(ui, |ui| {
                        ui.horizontal_wrapped(|ui| {
                            ui.vertical(|ui| {
                                ui.label(
                                    RichText::new(tr(&lang, "acpi_missing_title"))
                                        .color(Color32::from_rgb(254, 243, 199))
                                        .strong(),
                                );
                                ui.label(
                                    RichText::new(tr(&lang, "acpi_missing_desc"))
                                        .size(11.5)
                                        .color(Color32::from_rgb(253, 230, 138)),
                                );
                            });
                            if ui
                                .add(
                                    egui::Button::new(
                                        RichText::new(tr(&lang, "btn_install_acpi"))
                                            .color(Color32::WHITE)
                                            .strong(),
                                    )
                                    .fill(Color32::from_rgb(217, 119, 6))
                                    .min_size(Vec2::new(260.0, 30.0))
                                    .rounding(Rounding::same(6.0)),
                                )
                                .clicked()
                            {
                                let tx = self.tx.clone();
                                let ctx_clone = ctx.clone();
                                thread::spawn(move || {
                                    match backend::install_and_enable_acpi_call() {
                                        Ok(m) => {
                                            let _ = tx.send(AsyncMsg::Toast(m, true));
                                            let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                                        }
                                        Err(e) => {
                                            let _ = tx.send(AsyncMsg::Toast(e, false));
                                        }
                                    }
                                    ctx_clone.request_repaint();
                                });
                            }
                        });
                    });
                ui.add_space(6.0);
            }

            // 4. ACTIVE TAB CONTENT
            egui::ScrollArea::vertical().show(ui, |ui| match self.active_tab {
                0 => self.ui_tab_status(ui, ctx, &lang, accent_text, sec_text),
                1 => self.ui_tab_advanced(ui, &lang, accent_text, sec_text),
                2 => self.ui_tab_tools(ui, ctx, &lang, accent_text, sec_text),
                3 => self.ui_tab_logs(ui, &lang, accent_text),
                4 => self.ui_tab_settings(ui, ctx, &lang, accent_text, sec_text),
                _ => {}
            });
        });

        // Info Popup Modal if clicked
        if let Some(info_txt) = self.popup_info.clone() {
            let mut open = true;
            egui::Window::new("ℹ")
                .collapsible(false)
                .resizable(false)
                .open(&mut open)
                .show(ctx, |ui| {
                    ui.set_max_width(380.0);
                    ui.label(RichText::new(info_txt).size(13.0));
                });
            if !open {
                self.popup_info = None;
            }
        }
    }
}

impl ChargeLimiterApp {
    fn ui_tab_status(
        &mut self,
        ui: &mut egui::Ui,
        ctx: &egui::Context,
        lang: &str,
        accent_text: Color32,
        sec_text: Color32,
    ) {
        ui.columns(2, |cols| {
            // LEFT CARD: LIVE BATTERY TELEMETRY
            self.card_frame().show(&mut cols[0], |ui| {
                ui.label(RichText::new(tr(lang, "card_battery_title")).size(16.0).strong().color(accent_text));
                ui.add_space(6.0);

                self.surface_frame().show(ui, |ui| {
                    ui.horizontal(|ui| {
                        ui.label(RichText::new(format!("%{}", self.battery.percent)).size(38.0).strong());
                        ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                            let (st_txt, st_col) = match self.battery.state.as_str() {
                                "Charging" => (tr(lang, "state_charging"), Color32::from_rgb(16, 185, 129)),
                                "Not charging" => (tr(lang, "state_inhibited"), accent_text),
                                "Discharging" => (tr(lang, "state_discharging"), Color32::from_rgb(245, 158, 11)),
                                _ => (tr(lang, "state_unknown"), sec_text),
                            };
                            ui.label(RichText::new(st_txt).size(13.0).strong().color(st_col));
                        });
                    });
                    ui.add(
                        egui::ProgressBar::new(self.battery.percent as f32 / 100.0)
                            .desired_height(10.0)
                            .rounding(Rounding::same(5.0)),
                    );
                });

                ui.add_space(8.0);

                // 10 Telemetry Cells (Stacked Title + Wrapped Value)
                self.surface_frame().show(ui, |ui| {
                    let ac_val = if self.battery.ac == "online" {
                        tr(lang, "metric_ac_online")
                    } else {
                        tr(lang, "metric_ac_offline")
                    };
                    let bypass_val = if self.battery.bypass {
                        tr(lang, "metric_bypass_active")
                    } else {
                        tr(lang, "metric_bypass_inactive")
                    };

                    let rows = [
                        (("metric_ac", None), ac_val.to_string(), ("metric_power", None), self.battery.power.clone()),
                        (("metric_energy", None), self.battery.energy.clone(), ("metric_voltage", None), self.battery.voltage.clone()),
                        (("metric_health", None), self.battery.health.clone(), ("metric_cycles", None), self.battery.cycles.clone()),
                        (("metric_model", None), self.battery.pack_model.clone(), ("metric_bypass", None), bypass_val.to_string()),
                        (
                            ("metric_firmware", Some("tip_firmware")),
                            self.battery.firmware.clone(),
                            ("metric_last_off", Some("tip_last_off")),
                            self.battery.last_off.clone(),
                        ),
                    ];

                    for ((k1, tip1), v1, (k2, tip2), v2) in rows {
                        ui.columns(2, |c| {
                            c[0].horizontal(|ui| {
                                ui.label(RichText::new(tr(lang, k1)).size(11.5).color(sec_text));
                                if let Some(tk) = tip1 {
                                    self.info_badge(ui, tk);
                                }
                            });
                            c[0].label(RichText::new(v1).size(12.5).strong());

                            c[1].horizontal(|ui| {
                                ui.label(RichText::new(tr(lang, k2)).size(11.5).color(sec_text));
                                if let Some(tk) = tip2 {
                                    self.info_badge(ui, tk);
                                }
                            });
                            c[1].label(RichText::new(v2).size(12.5).strong());
                        });
                        ui.add_space(6.0);
                    }
                });
            });

            // RIGHT CARD: THRESHOLDS & CONTROLS
            self.card_frame().show(&mut cols[1], |ui| {
                ui.label(RichText::new(tr(lang, "card_limiter_title")).size(16.0).strong().color(accent_text));
                ui.add_space(6.0);

                // Quick Presets
                ui.horizontal(|ui| {
                    ui.label(RichText::new(tr(lang, "lbl_presets")).size(12.0).strong().color(sec_text));
                    self.info_badge(ui, "tip_presets");
                });
                ui.columns(3, |pcols| {
                    let presets = [
                        (60u8, 55u8, "preset_60"),
                        (80u8, 75u8, "preset_80"),
                        (90u8, 85u8, "preset_90"),
                    ];
                    for (idx, (st, sr, key)) in presets.iter().enumerate() {
                        let w = pcols[idx].available_width();
                        let active = self.svc_cfg.stop == *st && self.svc_cfg.start == *sr;
                        if self
                            .segmented_btn(&mut pcols[idx], tr(lang, key), active, Vec2::new(w, 28.0))
                            .clicked()
                        {
                            self.svc_cfg.stop = *st;
                            self.svc_cfg.start = *sr;
                        }
                    }
                });

                ui.add_space(8.0);

                // Stop Slider
                self.surface_frame().show(ui, |ui| {
                    ui.horizontal(|ui| {
                        let title = if lang.eq_ignore_ascii_case("en") {
                            format!("Max Charge Limit: %{} (Stop Charge)", self.svc_cfg.stop)
                        } else {
                            format!("Maksimum Şarj Limiti: %{} (Şarjı Durdur)", self.svc_cfg.stop)
                        };
                        ui.label(RichText::new(title).size(12.5).strong());
                        self.info_badge(ui, "tip_stop");
                    });
                    ui.label(RichText::new(tr(lang, "slider_stop_desc")).size(11.0).color(sec_text));
                    if ui
                        .add(egui::Slider::new(&mut self.svc_cfg.stop, 50..=95).show_value(false))
                        .changed()
                    {
                        if self.svc_cfg.start >= self.svc_cfg.stop {
                            self.svc_cfg.start = self.svc_cfg.stop.saturating_sub(5).max(40);
                        }
                    }
                });

                ui.add_space(6.0);

                // Start Slider
                self.surface_frame().show(ui, |ui| {
                    ui.horizontal(|ui| {
                        let title = if lang.eq_ignore_ascii_case("en") {
                            format!("Resume Threshold: %{} (Start Charge)", self.svc_cfg.start)
                        } else {
                            format!("Yeniden Başlama Eşiği: %{} (Şarja Başla)", self.svc_cfg.start)
                        };
                        ui.label(RichText::new(title).size(12.5).strong());
                        self.info_badge(ui, "tip_start");
                    });
                    ui.label(RichText::new(tr(lang, "slider_start_desc")).size(11.0).color(sec_text));
                    if ui
                        .add(egui::Slider::new(&mut self.svc_cfg.start, 40..=90).show_value(false))
                        .changed()
                    {
                        if self.svc_cfg.start >= self.svc_cfg.stop {
                            self.svc_cfg.start = self.svc_cfg.stop.saturating_sub(5).max(40);
                        }
                    }
                });

                ui.add_space(6.0);

                // Off-Mode Selector
                self.surface_frame().show(ui, |ui| {
                    ui.horizontal(|ui| {
                        ui.label(RichText::new(tr(lang, "lbl_off_mode")).size(12.5).strong());
                        self.info_badge(ui, "tip_off_mode");
                    });
                    let modes = ["hold", "inhibit", "auto", "keep"];
                    ui.columns(4, |mcols| {
                        for (i, m) in modes.iter().enumerate() {
                            let w = mcols[i].available_width();
                            if self
                                .segmented_btn(&mut mcols[i], m, self.svc_cfg.off_mode == *m, Vec2::new(w, 28.0))
                                .clicked()
                            {
                                self.svc_cfg.off_mode = m.to_string();
                            }
                        }
                    });
                    let desc_key = format!("off_mode_{}_desc", self.svc_cfg.off_mode);
                    ui.label(RichText::new(tr(lang, &desc_key)).size(11.0).color(accent_text));
                });

                ui.add_space(8.0);

                // Bypass Toggle
                ui.horizontal(|ui| {
                    let mut byp = self.battery.bypass;
                    if ui.checkbox(&mut byp, RichText::new(tr(lang, "switch_bypass")).strong()).changed() {
                        let tx = self.tx.clone();
                        let ctx_clone = ctx.clone();
                        thread::spawn(move || {
                            match backend::set_bypass(byp) {
                                Ok(m) => {
                                    let _ = tx.send(AsyncMsg::Toast(m, true));
                                    let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                                }
                                Err(e) => {
                                    let _ = tx.send(AsyncMsg::Toast(e, false));
                                }
                            }
                            ctx_clone.request_repaint();
                        });
                    }
                    self.info_badge(ui, "tip_bypass");
                });

                ui.add_space(6.0);

                // Save & Apply Button
                let w = ui.available_width();
                if ui
                    .add(
                        egui::Button::new(
                            RichText::new(tr(lang, "btn_save"))
                                .size(13.5)
                                .strong()
                                .color(Color32::WHITE),
                        )
                        .fill(Color32::from_rgb(2, 132, 199))
                        .min_size(Vec2::new(w, 36.0))
                        .rounding(Rounding::same(8.0)),
                    )
                    .clicked()
                {
                    let cfg_clone = self.svc_cfg.clone();
                    let mut gui_clone = self.gui_cfg.clone();
                    let tx = self.tx.clone();
                    let ctx_clone = ctx.clone();
                    thread::spawn(move || {
                        match backend::save_and_apply_service(&cfg_clone, &mut gui_clone) {
                            Ok(m) => {
                                let _ = tx.send(AsyncMsg::Toast(m, true));
                                let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                            }
                            Err(e) => {
                                let _ = tx.send(AsyncMsg::Toast(e, false));
                            }
                        }
                        ctx_clone.request_repaint();
                    });
                }
            });
        });
    }

    fn ui_tab_advanced(&mut self, ui: &mut egui::Ui, lang: &str, accent_text: Color32, sec_text: Color32) {
        self.card_frame().show(ui, |ui| {
            ui.label(RichText::new(tr(lang, "card_adv_title")).size(16.0).strong().color(accent_text));
            ui.add_space(8.0);

            // Run-Mode
            self.surface_frame().show(ui, |ui| {
                ui.horizontal(|ui| {
                    ui.label(RichText::new(tr(lang, "lbl_run_mode")).size(12.5).strong());
                    self.info_badge(ui, "tip_run_mode");
                });
                let modes = ["inhibit", "hold", "auto"];
                ui.columns(3, |mcols| {
                    for (i, m) in modes.iter().enumerate() {
                        let w = mcols[i].available_width();
                        if self
                            .segmented_btn(&mut mcols[i], m, self.svc_cfg.run_mode == *m, Vec2::new(w, 28.0))
                            .clicked()
                        {
                            self.svc_cfg.run_mode = m.to_string();
                        }
                    }
                });
                let desc_key = format!("run_mode_{}_desc", self.svc_cfg.run_mode);
                ui.label(RichText::new(tr(lang, &desc_key)).size(11.0).color(accent_text));
            });

            ui.add_space(8.0);

            // Off-Min Slider
            self.surface_frame().show(ui, |ui| {
                ui.horizontal(|ui| {
                    let title = if lang.eq_ignore_ascii_case("en") {
                        format!("Shutdown Low-Battery Guard: %{}", self.svc_cfg.off_min)
                    } else {
                        format!("Kapanışta Minimum Batarya Koruması: %{}", self.svc_cfg.off_min)
                    };
                    ui.label(RichText::new(title).size(12.5).strong());
                    self.info_badge(ui, "tip_off_min");
                });
                ui.label(RichText::new(tr(lang, "slider_off_min_desc")).size(11.0).color(sec_text));
                ui.add(egui::Slider::new(&mut self.svc_cfg.off_min, 0..=80).show_value(false));
            });

            ui.add_space(8.0);

            // Stop-Mode
            self.surface_frame().show(ui, |ui| {
                ui.horizontal(|ui| {
                    ui.label(RichText::new(tr(lang, "lbl_stop_mode")).size(12.5).strong());
                    self.info_badge(ui, "tip_stop_mode");
                });
                let modes = ["auto", "hold", "inhibit", "keep"];
                ui.columns(4, |mcols| {
                    for (i, m) in modes.iter().enumerate() {
                        let w = mcols[i].available_width();
                        if self
                            .segmented_btn(&mut mcols[i], m, self.svc_cfg.stop_mode == *m, Vec2::new(w, 28.0))
                            .clicked()
                        {
                            self.svc_cfg.stop_mode = m.to_string();
                        }
                    }
                });
                let desc_key = format!("stop_mode_{}_desc", self.svc_cfg.stop_mode);
                ui.label(RichText::new(tr(lang, &desc_key)).size(11.0).color(accent_text));
            });

            ui.add_space(8.0);

            // Interval Slider
            self.surface_frame().show(ui, |ui| {
                ui.horizontal(|ui| {
                    let title = if lang.eq_ignore_ascii_case("en") {
                        format!("Check Interval: {} seconds", self.svc_cfg.interval)
                    } else {
                        format!("Denetim Sıklığı: {} saniye", self.svc_cfg.interval)
                    };
                    ui.label(RichText::new(title).size(12.5).strong());
                    self.info_badge(ui, "tip_interval");
                });
                ui.label(RichText::new(tr(lang, "slider_interval_desc")).size(11.0).color(sec_text));
                ui.add(egui::Slider::new(&mut self.svc_cfg.interval, 5..=120).show_value(false));
            });

            ui.add_space(10.0);

            if self
                .segmented_btn(ui, tr(lang, "btn_reset_defaults"), false, Vec2::new(260.0, 32.0))
                .clicked()
            {
                self.svc_cfg.stop = 80;
                self.svc_cfg.start = 75;
                self.svc_cfg.interval = 30;
                self.svc_cfg.run_mode = "inhibit".to_string();
                self.svc_cfg.off_mode = "hold".to_string();
                self.svc_cfg.off_min = 40;
                self.svc_cfg.stop_mode = "auto".to_string();
            }
        });
    }

    fn ui_tab_tools(
        &mut self,
        ui: &mut egui::Ui,
        ctx: &egui::Context,
        lang: &str,
        accent_text: Color32,
        sec_text: Color32,
    ) {
        ui.columns(2, |cols| {
            // LEFT: WATCH & MANUAL OVERRIDE
            self.card_frame().show(&mut cols[0], |ui| {
                ui.label(RichText::new(tr(lang, "card_watch_title")).size(15.0).strong().color(accent_text));
                ui.label(RichText::new(tr(lang, "card_watch_desc")).size(11.5).color(sec_text));
                ui.add_space(6.0);

                ui.horizontal(|ui| {
                    for s in [10u64, 30, 60] {
                        if self
                            .segmented_btn(ui, &format!("{}s", s), self.watch_secs == s, Vec2::new(50.0, 28.0))
                            .clicked()
                        {
                            self.watch_secs = s;
                        }
                    }
                    if !self.watch_running {
                        if self
                            .segmented_btn(ui, tr(lang, "btn_start_watch"), true, Vec2::new(120.0, 28.0))
                            .clicked()
                        {
                            self.watch_running = true;
                            self.watch_output = "Sampling battery energy...".to_string();
                            let secs = self.watch_secs;
                            let tx = self.tx.clone();
                            let ctx_clone = ctx.clone();
                            thread::spawn(move || {
                                let out = backend::run_watch_test(secs);
                                let _ = tx.send(AsyncMsg::WatchOutput(out));
                                ctx_clone.request_repaint();
                            });
                        }
                    }
                });

                ui.add_space(6.0);
                egui::ScrollArea::vertical()
                    .id_salt("watch_scroll")
                    .max_height(150.0)
                    .show(ui, |ui| {
                        ui.add(
                            egui::TextEdit::multiline(&mut self.watch_output)
                                .font(FontId::monospace(11.0))
                                .desired_width(f32::INFINITY)
                                .interactive(false),
                        );
                    });

                ui.add_space(10.0);
                ui.label(RichText::new(tr(lang, "card_manual_title")).size(15.0).strong().color(accent_text));
                ui.label(RichText::new(tr(lang, "card_manual_desc")).size(11.5).color(sec_text));
                ui.add_space(6.0);

                ui.columns(3, |mcols| {
                    let btns = [
                        ("inhibit", "btn_set_inhibit"),
                        ("hold", "btn_set_hold"),
                        ("auto", "btn_set_auto"),
                    ];
                    for (i, (m, k)) in btns.iter().enumerate() {
                        let w = mcols[i].available_width();
                        if self
                            .segmented_btn(&mut mcols[i], tr(lang, k), false, Vec2::new(w, 30.0))
                            .clicked()
                        {
                            let mode = m.to_string();
                            let tx = self.tx.clone();
                            let ctx_clone = ctx.clone();
                            thread::spawn(move || {
                                match backend::set_manual_mode(&mode) {
                                    Ok(msg) => {
                                        let _ = tx.send(AsyncMsg::Toast(msg, true));
                                        let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                                    }
                                    Err(e) => {
                                        let _ = tx.send(AsyncMsg::Toast(e, false));
                                    }
                                }
                                ctx_clone.request_repaint();
                            });
                        }
                    }
                });
            });

            // RIGHT: DIAGNOSE & ACPI_CALL INSTALLER
            self.card_frame().show(&mut cols[1], |ui| {
                ui.label(RichText::new(tr(lang, "card_diag_title")).size(15.0).strong().color(accent_text));
                ui.label(RichText::new(tr(lang, "card_diag_desc")).size(11.5).color(sec_text));
                ui.add_space(6.0);

                ui.horizontal(|ui| {
                    if self
                        .segmented_btn(ui, tr(lang, "btn_run_diag"), true, Vec2::new(160.0, 30.0))
                        .clicked()
                    {
                        let tx = self.tx.clone();
                        let ctx_clone = ctx.clone();
                        thread::spawn(move || {
                            let out = backend::run_diagnose();
                            let _ = tx.send(AsyncMsg::DiagOutput(out));
                            ctx_clone.request_repaint();
                        });
                    }

                    if self
                        .segmented_btn(ui, tr(lang, "btn_install_acpi"), false, Vec2::new(220.0, 30.0))
                        .clicked()
                    {
                        let tx = self.tx.clone();
                        let ctx_clone = ctx.clone();
                        thread::spawn(move || {
                            match backend::install_and_enable_acpi_call() {
                                Ok(m) => {
                                    let _ = tx.send(AsyncMsg::Toast(m, true));
                                    let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                                }
                                Err(e) => {
                                    let _ = tx.send(AsyncMsg::Toast(e, false));
                                }
                            }
                            ctx_clone.request_repaint();
                        });
                    }
                });

                ui.add_space(8.0);
                egui::ScrollArea::vertical()
                    .id_salt("diag_scroll")
                    .max_height(260.0)
                    .show(ui, |ui| {
                        ui.add(
                            egui::TextEdit::multiline(&mut self.diag_output)
                                .font(FontId::monospace(11.0))
                                .desired_width(f32::INFINITY)
                                .interactive(false),
                        );
                    });
            });
        });
    }

    fn ui_tab_logs(&mut self, ui: &mut egui::Ui, lang: &str, accent_text: Color32) {
        self.card_frame().show(ui, |ui| {
            ui.horizontal(|ui| {
                ui.label(RichText::new(tr(lang, "lbl_log_title")).size(15.0).strong().color(accent_text));
                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if self
                        .segmented_btn(ui, tr(lang, "btn_clear_log"), false, Vec2::new(80.0, 26.0))
                        .clicked()
                    {
                        if backend::clear_daemon_log().is_ok() {
                            self.log_text.clear();
                        }
                    }
                    if self
                        .segmented_btn(ui, tr(lang, "btn_open_folder"), false, Vec2::new(95.0, 26.0))
                        .clicked()
                    {
                        backend::open_log_folder();
                    }
                    if self
                        .segmented_btn(ui, tr(lang, "btn_copy_log"), false, Vec2::new(110.0, 26.0))
                        .clicked()
                    {
                        ui.ctx().copy_text(self.log_text.clone());
                    }
                    ui.checkbox(&mut self.live_tail, tr(lang, "chk_live_tail"));
                });
            });
            ui.add_space(6.0);
            egui::ScrollArea::vertical()
                .id_salt("logs_scroll")
                .max_height(360.0)
                .show(ui, |ui| {
                    ui.add(
                        egui::TextEdit::multiline(&mut self.log_text)
                            .font(FontId::monospace(11.5))
                            .desired_width(f32::INFINITY)
                            .interactive(false),
                    );
                });
        });
    }

    fn ui_tab_settings(
        &mut self,
        ui: &mut egui::Ui,
        ctx: &egui::Context,
        lang: &str,
        accent_text: Color32,
        sec_text: Color32,
    ) {
        ui.columns(2, |cols| {
            self.card_frame().show(&mut cols[0], |ui| {
                ui.label(RichText::new(tr(lang, "card_pref_title")).size(15.5).strong().color(accent_text));
                ui.add_space(8.0);

                if ui
                    .checkbox(&mut self.gui_cfg.autostart_gui, tr(lang, "sw_autostart"))
                    .changed()
                {
                    backend::set_autostart_gui(self.gui_cfg.autostart_gui);
                    backend::save_gui_config(&self.gui_cfg);
                }
                ui.label(RichText::new(tr(lang, "autostart_info")).size(11.5).color(sec_text));

                ui.add_space(12.0);
                let w = ui.available_width();
                if self
                    .segmented_btn(ui, tr(lang, "btn_install_acpi"), true, Vec2::new(w, 34.0))
                    .clicked()
                {
                    let tx = self.tx.clone();
                    let ctx_clone = ctx.clone();
                    thread::spawn(move || {
                        match backend::install_and_enable_acpi_call() {
                            Ok(m) => {
                                let _ = tx.send(AsyncMsg::Toast(m, true));
                                let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                            }
                            Err(e) => {
                                let _ = tx.send(AsyncMsg::Toast(e, false));
                            }
                        }
                        ctx_clone.request_repaint();
                    });
                }
            });

            self.card_frame().show(&mut cols[1], |ui| {
                ui.label(RichText::new(tr(lang, "card_service_mgr_title")).size(15.5).strong().color(accent_text));
                ui.add_space(8.0);

                ui.columns(3, |scols| {
                    let actions = [
                        ("restart", "btn_restart_svc"),
                        ("stop", "btn_stop_svc"),
                        ("start", "btn_start_svc"),
                    ];
                    for (i, (act, key)) in actions.iter().enumerate() {
                        let w = scols[i].available_width();
                        if self
                            .segmented_btn(&mut scols[i], tr(lang, key), false, Vec2::new(w, 30.0))
                            .clicked()
                        {
                            let a = act.to_string();
                            let tx = self.tx.clone();
                            let ctx_clone = ctx.clone();
                            thread::spawn(move || {
                                let _ = backend::control_systemd_service(&a);
                                let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                                ctx_clone.request_repaint();
                            });
                        }
                    }
                });

                ui.add_space(8.0);
                let w = ui.available_width();
                if self
                    .segmented_btn(ui, tr(lang, "btn_uninstall_svc"), false, Vec2::new(w, 30.0))
                    .clicked()
                {
                    let tx = self.tx.clone();
                    let ctx_clone = ctx.clone();
                    thread::spawn(move || {
                        match backend::uninstall_service() {
                            Ok(m) => {
                                let _ = tx.send(AsyncMsg::Toast(m, true));
                                let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                            }
                            Err(e) => {
                                let _ = tx.send(AsyncMsg::Toast(e, false));
                            }
                        }
                        ctx_clone.request_repaint();
                    });
                }

                ui.add_space(6.0);
                if ui
                    .add(
                        egui::Button::new(
                            RichText::new(tr(lang, "btn_full_cleanup"))
                                .size(13.0)
                                .strong()
                                .color(Color32::WHITE),
                        )
                        .fill(Color32::from_rgb(220, 38, 38))
                        .min_size(Vec2::new(w, 34.0))
                        .rounding(Rounding::same(6.0)),
                    )
                    .clicked()
                {
                    let tx = self.tx.clone();
                    let ctx_clone = ctx.clone();
                    thread::spawn(move || {
                        match backend::full_system_cleanup() {
                            Ok(m) => {
                                let _ = tx.send(AsyncMsg::Toast(m, true));
                                let _ = tx.send(AsyncMsg::BatteryUpdated(backend::get_battery_status()));
                            }
                            Err(e) => {
                                let _ = tx.send(AsyncMsg::Toast(e, false));
                            }
                        }
                        ctx_clone.request_repaint();
                    });
                }

                ui.add_space(10.0);
                self.surface_frame().show(ui, |ui| {
                    ui.label(RichText::new(tr(lang, "card_about_title")).size(12.5).strong());
                    ui.label(RichText::new(tr(lang, "about_text")).size(11.5).color(sec_text));
                });
            });
        });
    }
}

fn main() -> eframe::Result<()> {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_inner_size([1040.0, 740.0])
            .with_min_inner_size([920.0, 640.0])
            .with_title("HP Battery Charge Limiter"),
        ..Default::default()
    };

    eframe::run_native(
        "HP Battery Charge Limiter",
        options,
        Box::new(|cc| Ok(Box::new(ChargeLimiterApp::new(cc)))),
    )
}
