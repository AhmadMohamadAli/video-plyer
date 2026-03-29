# 🎬 Lumière Player

A sleek, dark-themed desktop video player for Windows built with **WPF (.NET 8)** and **LibVLCSharp**. Supports local files, network streams, playlists, and swipe-to-seek.

---

## ✨ Features

| Feature | Details |
|---|---|
| 🎥 Video Playback | Plays MP4, MKV, AVI, WMV, WebM, and more via LibVLC |
| 🎵 Audio Playback | MP3, WAV, FLAC, OGG |
| 📋 Playlist | Add multiple files, click to play from queue |
| 🌐 Stream URL | Paste any HTTP/RTSP/YouTube stream URL and play |
| ⏱️ Timeline | Scrubable progress bar with live time display (00:00 / 05:32) |
| 🔊 Volume Control | Gold-styled slider with instant feedback |
| ↔️ Swipe Seek | Click and drag on the video to seek forward/backward |
| ⛶ Fullscreen | Double-click or button; Escape to exit |
| ⌨️ Keyboard Shortcuts | Space = play/pause, ← / → = ±10s, Escape = exit fullscreen |

---

## 🖥️ Requirements

- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [LibVLC](https://www.videolan.org/vlc/libvlc.html) (installed automatically via NuGet)

---

## 🚀 Getting Started

```bash
# Clone the repository
git clone https://github.com/AhmadMohamadAli/video-plyer.git
cd video-plyer

# Restore packages and run
dotnet restore
dotnet run
```

Or open `video plyer.slnx` in Visual Studio 2022+ and press **F5**.

---

## ⌨️ Keyboard Shortcuts

| Key | Action |
|---|---|
| `Space` | Play / Pause |
| `←` | Seek back 10 seconds |
| `→` | Seek forward 10 seconds |
| `Escape` | Exit fullscreen |

---

## 📁 Project Structure

```
video-plyer/
├── MainWindow.xaml        # UI layout and styles
├── MainWindow.xaml.cs     # Playback logic and event handlers
├── App.xaml / App.xaml.cs # Application entry point
├── AssemblyInfo.cs
└── video plyer.csproj     # Project file (targets net8.0-windows)
```

---

## 🛠️ Dependencies

| Package | Purpose |
|---|---|
| `LibVLCSharp` | Core media engine |
| `LibVLCSharp.WPF` | WPF VideoView control |
| `VideoLAN.LibVLC.Windows` | Native LibVLC binaries |

Install via NuGet:

```bash
dotnet add package LibVLCSharp
dotnet add package LibVLCSharp.WPF
dotnet add package VideoLAN.LibVLC.Windows
```

---

## 📝 License

MIT — free to use, modify, and distribute.
