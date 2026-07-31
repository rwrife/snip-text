# snip-text — Project Plan

## Scope

A small, focused Windows 10/11 desktop utility that captures a user-drawn screen region, runs OCR on it, and delivers editable text to the clipboard via a global hotkey. Offline-first using the native Windows OCR engine, with an **optional** local-AI vision pass for hard cases. No cloud dependency for core value.

**In scope**
- Global hotkey to trigger a full-screen dimmed capture overlay with rubber-band selection.
- Multi-monitor and per-monitor DPI aware capture.
- OCR via `Windows.Media.Ocr` (offline, language packs the user already has installed).
- Clipboard output + a small editable preview toast.
- Settings: hotkey, OCR language, output behavior, optional local-AI endpoint.
- Optional local-AI vision engine (Ollama / llama.cpp) with graceful fallback.
- Windows packaging (portable self-contained zip + MSIX).

## Architecture / tech approach

- **Platform:** .NET 8, C#, **WPF** desktop app; runs as a tray application (`NotifyIcon` via `H.NotifyIcon` or WinForms interop).
- **Global hotkey:** Win32 `RegisterHotKey` / `WM_HOTKEY` message hook.
- **Capture overlay:** borderless topmost transparent WPF window spanning the full virtual desktop; rubber-band `Rectangle`; captures pixels via `Graphics.CopyFromScreen` / `BitBlt` into a bitmap. Per-monitor DPI awareness (`PerMonitorV2`) so coordinates and crops stay pixel-accurate.
- **OCR engine (default):** `Windows.Media.Ocr.OcrEngine` from the Windows Runtime (`Microsoft.Windows.SDK.Contracts` / CsWinRT). Convert the crop to `SoftwareBitmap`, recognize, reconstruct line/word layout, join with newlines.
- **Local-AI engine (optional):** abstraction `ITextRecognizer` with two implementations — `WindowsOcrRecognizer` and `LocalAiVisionRecognizer`. The AI one base64-encodes the crop and POSTs to an OpenAI-compatible `/v1/chat/completions` (vision) endpoint on `localhost`. Timeout + reachability check → **graceful fallback** to Windows OCR. Off by default.
- **Clipboard + preview:** copy recognized text to clipboard; show a lightweight editable toast (WPF window) allowing quick correction before re-copy.
- **Persistence:** JSON settings file under `%APPDATA%\snip-text\settings.json`.
- **Testing:** xUnit. Pure logic (layout reconstruction, settings, recognizer selection/fallback, coordinate/DPI math) unit-tested; recognizer interface mocked so tests run headless in CI.

### Suggested layout
```
src/SnipText/            # WPF app (UI, tray, overlay, hotkey)
src/SnipText.Core/       # recognizers, OCR, clipboard, settings (testable, UI-free)
tests/SnipText.Tests/    # xUnit
```

## Milestones

- **M1 — Capture core:** tray app, global hotkey, dimmed multi-monitor overlay, rubber-band selection, pixel-accurate DPI-aware crop to bitmap.
- **M2 — Native OCR:** `WindowsOcrRecognizer`, layout reconstruction, clipboard output, first end-to-end grab.
- **M3 — UX:** editable preview toast, settings window (hotkey, language, output mode), first-run experience.
- **M4 — Local-AI:** `ITextRecognizer` abstraction, `LocalAiVisionRecognizer` (Ollama/llama.cpp), reachability probe + graceful fallback, confidence-based routing.
- **M5 — Ship:** xUnit coverage on core, CI build, portable self-contained x64 zip + MSIX packaging, README quickstart, first Release.

## Non-goals

- No cloud OCR services or accounts; no telemetry.
- No PDF/document management — snip-text grabs a screen region, it is not a document editor.
- No translation, TTS, or general note-taking (out of scope for v1).
- No macOS/Linux port (Windows-first; native OCR is Windows-specific).
- No always-on screen scraping/monitoring — capture is explicit and user-triggered.

## Packaging target for Windows

- **Primary:** portable **self-contained x64** build (`dotnet publish -r win-x64 --self-contained`) zipped as `snip-text-win-x64.zip` — no install, run the exe.
- **Secondary:** **MSIX** package for Start-menu install and clean uninstall.
- Published via GitHub Releases as milestones complete.
