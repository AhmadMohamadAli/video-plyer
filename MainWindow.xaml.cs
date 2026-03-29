using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaApp
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindow : Window
    {
        private LibVLC? _libVLC;
        private MediaPlayer? _mediaPlayer;
        private bool _isFullscreen = false;

        private readonly DispatcherTimer _timer;
        private bool _isDragging = false;

        // Swipe seek
        private Point _swipeStartPoint;
        private bool _isSwiping = false;
        private const double SwipePixelsPerSecond = 5.0; // px movement = 1 second seek

        public MainWindow()
        {
            InitializeComponent();

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            VideoView.MediaPlayer = _mediaPlayer;

            // Set initial volume to match the slider default
            _mediaPlayer.Volume = (int)VolumeSlider.Value;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            Closed += MainWindow_Closed;
        }

        // ── Resource cleanup ──────────────────────────────────────────────

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _timer.Stop();
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        // ── Timeline update ───────────────────────────────────────────────

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_mediaPlayer is null || !_mediaPlayer.IsPlaying || _isDragging)
                return;

            long length = _mediaPlayer.Length;
            long time   = _mediaPlayer.Time;

            if (length > 0)
            {
                TimelineSlider.Value = (double)time / length * 100.0;
                TimeLabel.Text = $"{FormatMs(time)} / {FormatMs(length)}";
            }
        }

        private static string FormatMs(long ms)
        {
            var t = TimeSpan.FromMilliseconds(ms);
            return t.Hours > 0
                ? $"{t.Hours}:{t.Minutes:D2}:{t.Seconds:D2}"
                : $"{t.Minutes:D2}:{t.Seconds:D2}";
        }

        private void Timeline_PreviewMouseDown(object sender, MouseButtonEventArgs e)
            => _isDragging = true;

        private void Timeline_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mediaPlayer?.Length > 0)
                _mediaPlayer.Time = (long)(TimelineSlider.Value / 100.0 * _mediaPlayer.Length);

            _isDragging = false;
        }

        // ── Add files ─────────────────────────────────────────────────────

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Media|*.mp3;*.mp4;*.avi;*.mkv;*.wmv;*.wav;*.flac;*.ogg;*.webm",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
                foreach (var file in dialog.FileNames)
                    PlaylistBox.Items.Add(file);
        }

        // ── Playlist ──────────────────────────────────────────────────────

        private void PlaylistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_mediaPlayer is null || PlaylistBox.SelectedItem is not string path
                || string.IsNullOrWhiteSpace(path))
                return;

            _mediaPlayer.Play(new Media(_libVLC!, path, FromType.FromPath));
            TimeLabel.Text = "00:00 / 00:00";
        }

        // ── Playback controls ─────────────────────────────────────────────

        private void Back10_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer is null) return;
            _mediaPlayer.Time = Math.Max(0, _mediaPlayer.Time - 10_000);
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer is null) return;
            if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause();
            else _mediaPlayer.Play();
        }

        private void Forward10_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer is null) return;
            _mediaPlayer.Time = Math.Min(_mediaPlayer.Length, _mediaPlayer.Time + 10_000);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer?.Stop();
            TimelineSlider.Value = 0;
            TimeLabel.Text = "00:00 / 00:00";
        }

        private void PlayUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(url) || _mediaPlayer is null) return;
            _mediaPlayer.Play(new Media(_libVLC!, url, FromType.FromLocation));
        }

        // ── Fullscreen ────────────────────────────────────────────────────

        private void Fullscreen_Click(object? sender, RoutedEventArgs? e)
        {
            if (!_isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode  = ResizeMode.NoResize;
                Topmost     = true;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode  = ResizeMode.CanResize;
                Topmost     = false;
            }
            _isFullscreen = !_isFullscreen;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Key.Escape when _isFullscreen:
                    Fullscreen_Click(null, null);
                    break;
                case Key.Space:
                    PlayPause_Click(null!, null!);
                    e.Handled = true;
                    break;
                case Key.Left:
                    Back10_Click(null!, null!);
                    e.Handled = true;
                    break;
                case Key.Right:
                    Forward10_Click(null!, null!);
                    e.Handled = true;
                    break;
            }
        }

        private void VideoView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Fullscreen_Click(null, null);
        }

        // ── Swipe seek ────────────────────────────────────────────────────
        // Each SwipePixelsPerSecond pixels of horizontal drag = 1 second seek.

        private void Video_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _swipeStartPoint = e.GetPosition(this);
            _isSwiping = true;
        }

        private void Video_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSwiping || _mediaPlayer is null || _mediaPlayer.Length <= 0) return;

            Point current = e.GetPosition(this);
            double deltaX = current.X - _swipeStartPoint.X;

            // Convert pixels → milliseconds
            long deltaMs = (long)(deltaX / SwipePixelsPerSecond * 1000.0);
            long newTime  = Math.Clamp(_mediaPlayer.Time + deltaMs, 0, _mediaPlayer.Length);

            _mediaPlayer.Time = newTime;
            _swipeStartPoint  = current;
        }

        private void Video_MouseUp(object sender, MouseButtonEventArgs e)
            => _isSwiping = false;

        // ── Volume ────────────────────────────────────────────────────────

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer is not null)
                _mediaPlayer.Volume = (int)VolumeSlider.Value;
        }
    }
}
