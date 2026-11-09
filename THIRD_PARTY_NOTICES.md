# Third-Party Notices & License Compliance

This project incorporates and builds upon open-source software. In accordance with the **MIT License** and **Apache License, Version 2.0 (Section 4)**, the following attributions, copyright notices, and license terms apply:

---

## 1. Core Hardware Engine: `hp-charge-limit-rs`

* **Project Name:** `hp-charge-limit-rs`
* **Author / Maintainer:** Onur Kral ([@kralonur](https://github.com/kralonur)) & `hp-charge-limit-rs` contributors
* **Official Repository:** https://github.com/kralonur/hp-charge-limit-rs
* **Version Included:** `v0.4.1` (`3cdff25dd11b05797b8f56a393a58fcc3e8d3a2c`)
* **License:** Dual-licensed under the **MIT License** ([LICENSE-MIT](LICENSE-MIT)) or **Apache License, Version 2.0** ([LICENSE-APACHE](LICENSE-APACHE)).
* **Source Location in Repo:** [`hp-charge-limit-rs-source/`](hp-charge-limit-rs-source/)

### Rust Crate Dependencies of `hp-charge-limit-rs` (Windows Target)
1. **`anyhow` (v1.0.104)** — MIT OR Apache-2.0 (Copyright (c) David Tolnay)
2. **`clap` (v4.6.7) & `clap_complete` (v4.6)** — MIT OR Apache-2.0 (Copyright (c) Kevin K., clap-rs contributors)
3. **`windows` (v0.62.2)** — MIT OR Apache-2.0 (Copyright (c) Microsoft Corporation)
4. **`windows-service` (v0.8.1)** — MIT OR Apache-2.0 (Copyright (c) Mullvad VPN AB)

---

## 2. GUI Editions Included in This Repository

### v2.0.0 — Native GPU-Accelerated WPF Edition (`src/v2-native-wpf/`)
* **Technology:** Native C# / .NET Framework WPF (`PresentationFramework`, `PresentationCore`, `WindowsBase`, `System.Drawing`, `System.Windows.Forms`)
* **License:** MIT OR Apache-2.0 (matching upstream license)
* **Third-Party Libraries:** None (100% native Windows OS components)

### v1.0.0 — Python CustomTkinter Edition (`hp-charge-limiter-gui/`)
* **Technology:** Python 3, `customtkinter` (MIT License, Copyright (c) Tom Schimansky), `pystray` (LGPLv3 / MIT), `Pillow` (HPND License).
* **License:** MIT OR Apache-2.0

---

## 3. Compliance Summary (MIT & Apache-2.0)

1. **License Preservation:** Both [LICENSE-MIT](LICENSE-MIT) and [LICENSE-APACHE](LICENSE-APACHE) are preserved in full in the root directory of this repository.
2. **Attribution & Notice:** The [NOTICE](NOTICE) file documents the original copyright holders and clearly distinguishes the unmodified upstream Rust core (`hp-charge-limit-rs`) from the newly added desktop GUI layers (`v1.0.0` and `v2.0.0`).
3. **Disclaimer of Warranty:** As stated in both licenses, the software is provided "AS IS", without warranty of any kind.
