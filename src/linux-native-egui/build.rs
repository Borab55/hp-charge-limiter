use std::env;
use std::fs;
use std::path::PathBuf;

fn main() {
    let out_dir = PathBuf::from(env::var("OUT_DIR").unwrap());
    let dest = out_dir.join("embedded_core_engine");

    let candidate = PathBuf::from("core/hp-charge-limit-rs");
    println!("cargo:rerun-if-changed=core/hp-charge-limit-rs");

    if candidate.exists() {
        fs::copy(&candidate, &dest).expect("Failed to copy core/hp-charge-limit-rs");
    } else {
        // Fallback placeholder if compiled without pre-built core binary
        fs::write(&dest, b"").expect("Failed to write placeholder");
    }
}
