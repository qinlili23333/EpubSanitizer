@echo off
echo Build libcaesium with native architecture...
rem Use native architecture (should be AVX512 on my laptop)
git clone -b 0.18.1 https://github.com/Lymphatus/libcaesium.git
set RUSTFLAGS=-C target-cpu=native -C target-feature=+crt-static
cd libcaesium
git rev-list HEAD --count > ..\libcaesium_version.txt
cargo build --release
xcopy target\release\caesium.dll .. /Y