# snip-text

**Windows region-capture OCR — snip any part of your screen and instantly grab editable text to your clipboard.** Offline by default, with optional local-AI vision for tough layouts and handwriting. Privacy-first.

## Overview

`snip-text` is a lightweight Windows 10/11 desktop utility that lets you draw a rectangle over anything on your screen — a PDF you can't select, an error dialog, a screenshot someone sent you, a photo of a whiteboard — and pulls the text straight out into your clipboard. Press a global hotkey, drag a box, and the recognized text is ready to paste.

Recognition runs **fully offline** using the built-in Windows OCR engine (`Windows.Media.Ocr`). No image ever leaves your machine. For difficult cases — dense tables, mixed columns, low-contrast scans, or handwriting — you can optionally point snip-text at a **local tiny vision model** (MiniCPM-V class via Ollama or llama.cpp) for a smarter pass, still 100% on-device.

## Motivation

Text trapped in images is a daily annoyance: baked-into-PDF scans, screenshots, screen-shared slides, license keys in dialog boxes, code in a YouTube thumbnail. Copy/paste doesn't work, so you retype — slowly and with errors.

Windows ships a capable OCR engine, but it's buried behind an API with no friendly front-end. snip-text turns it into a one-hotkey action. And because everything runs locally, it's safe for sensitive material (credentials, contracts, internal docs) that you'd never paste into a cloud OCR site.

## Use cases

- **Grab text from a scanned PDF** that has no selectable text layer.
- **Copy an error message** out of a dialog box that blocks text selection.
- **Extract a code snippet** from a screenshot, video still, or screen share.
- **Pull a license key / OTP / URL** out of an image without retyping.
- **Digitize a whiteboard photo** or handwritten note (local-AI mode).
- **Lift a table** from a screenshot into tab-separated text for a spreadsheet.

## How to use (Windows-first quickstart)

> Requires Windows 10 (1809+) or Windows 11. Prebuilt portable zip and MSIX will be published under Releases as milestones land.

1. Download the latest `snip-text-win-x64.zip` from Releases and unzip, **or** build from source (see below).
2. Run `snip-text.exe`. It lives in the system tray.
3. Press the global hotkey (default **Ctrl+Shift+O**) — the screen dims and the crosshair appears.
4. Drag a rectangle over the text you want.
5. Recognized text is copied to your clipboard and shown in a small preview toast where you can edit before pasting.

### Build from source

```powershell
git clone https://github.com/rwrife/snip-text.git
cd snip-text
dotnet build src/SnipText/SnipText.csproj -c Release
dotnet run --project src/SnipText/SnipText.csproj
```

## CI and release packaging

### CI (push + pull request)

GitHub Actions runs on every push and pull request via `.github/workflows/ci.yml`:

- Build the WPF app (`src/SnipText/SnipText.csproj`)
- Run xUnit tests (`tests/SnipText.Tests/SnipText.Tests.csproj`)

### Portable self-contained zip (win-x64)

The release workflow `.github/workflows/release-windows.yml` publishes a self-contained Windows x64 build and zips it as:

- `snip-text-win-x64.zip`

You can produce the same artifact locally on Windows:

```powershell
pwsh ./scripts/publish-win-x64.ps1 -Configuration Release
```

When a `v*` tag is pushed, the workflow automatically attaches the zip to the corresponding GitHub Release.

Tagging example:

```bash
git tag v0.1.0
git push origin v0.1.0
```

### MSIX packaging config

MSIX packaging scaffolding lives under `packaging/SnipText.Package/`:

- `SnipText.Package.wapproj`
- `Package.appxmanifest`

Build locally on Windows with Visual Studio Build Tools / MSBuild:

```powershell
pwsh ./scripts/build-msix.ps1 -Configuration Release -Platform x64
```

> Note: replace the placeholder assets under `packaging/SnipText.Package/Assets/` and configure signing as part of release hardening.

## Example workflow

```text
# Grab a code block from a screen-shared slide
1. Ctrl+Shift+O
2. Drag a box around the code
3. Paste into your editor  ->  text arrives with line breaks preserved

# Digitize a handwritten sticky note (local-AI mode on)
1. Ctrl+Shift+O
2. Box the note
3. snip-text sends the crop to your local MiniCPM-V model
4. Cleaned-up transcription lands on the clipboard
```

## Local-AI integration (optional)

snip-text works with **zero AI** out of the box using the native Windows OCR engine. When you want a smarter pass, enable local-AI mode in Settings:

- Point it at an **Ollama** (`http://localhost:11434`) or **llama.cpp** OpenAI-compatible endpoint.
- Recommended tiny vision models: **MiniCPM-V** family, or any small llava-class model your machine can run.
- Use it as a fallback (only when native OCR confidence is low) or as the primary engine for handwriting/complex layouts.
- **Nothing is sent to the cloud** — the model runs on your own hardware. If no local model is reachable, snip-text silently falls back to native OCR.

## Current status / milestones

Early scaffold. Tracked in `PLAN.md` and the issue backlog:

- [ ] M1 — Region-capture overlay + global hotkey
- [ ] M2 — Native Windows OCR pipeline + clipboard output
- [ ] M3 — Edit/preview toast + settings
- [ ] M4 — Optional local-AI vision engine
- [ ] M5 — Packaging (portable zip + MSIX) & tests

Contributions and issue-by-issue progress welcome. See open issues.
