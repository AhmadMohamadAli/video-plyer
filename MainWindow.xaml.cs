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
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        private bool isFullscreen = false;

        // 🔥 NEW
        private DispatcherTimer timer;
        private bool isDragging = false;

        private Point startPoint;
        private bool isSwiping = false;

        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            VideoView.MediaPlayer = _mediaPlayer;

            // 🔥 TIMER FOR TIMELINE
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // -----------------------------
        // TIMELINE UPDATE
        // -----------------------------
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_mediaPlayer == null || !_mediaPlayer.IsPlaying || isDragging)
                return;

            if (_mediaPlayer.Length > 0)
            {
                TimelineSlider.Value =
                    (double)_mediaPlayer.Time / _mediaPlayer.Length * 100;
            }
        }

        private void Timeline_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Timeline_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mediaPlayer.Length > 0)
            {
                double pos = TimelineSlider.Value / 100.0;
                _mediaPlayer.Time = (long)(pos * _mediaPlayer.Length);
            }

            isDragging = false;
        }

        // -----------------------------
        // ADD FILES
        // -----------------------------
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Media|*.mp3;*.mp4;*.avi;*.mkv;*.wmv;*.wav",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                    PlaylistBox.Items.Add(file);
            }
        }

        // -----------------------------
        // PLAY FROM PLAYLIST
        // -----------------------------
        private void PlaylistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistBox.SelectedItem == null) return;

            string? path = PlaylistBox.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(path)) return;

            _mediaPlayer.Play(new Media(_libVLC, path, FromType.FromPath));
        }

        // -----------------------------
        // CONTROLS
        // -----------------------------
        private void Back10_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Time -= 10000;
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Pause();
            else
                _mediaPlayer.Play();
        }

        private void Forward10_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Time += 10000;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
        }

        private void PlayUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(url)) return;

            _mediaPlayer.Play(new Media(_libVLC, url, FromType.FromLocation));
        }

        // -----------------------------
        // FULLSCREEN
        // -----------------------------
        private void Fullscreen_Click(object? sender, RoutedEventArgs? e)
        {
            if (!isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;
            }

            isFullscreen = !isFullscreen;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && isFullscreen)
                Fullscreen_Click(null, null);
        }

        // 🔥 DOUBLE CLICK FULLSCREEN
        private void VideoView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Fullscreen_Click(null, null);
        }

        // -----------------------------
        // SWIPE SEEK
        // -----------------------------
        private void Video_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            isSwiping = true;
        }

        private void Video_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSwiping || _mediaPlayer.Length <= 0) return;

            Point current = e.GetPosition(this);
            double deltaX = current.X - startPoint.X;

            double percent = deltaX / 500.0;

            long newTime = _mediaPlayer.Time + (long)(percent * _mediaPlayer.Length);

            if (newTime < 0) newTime = 0;
            if (newTime > _mediaPlayer.Length) newTime = _mediaPlayer.Length;

            _mediaPlayer.Time = newTime;

            startPoint = current;
        }

        private void Video_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isSwiping = false;
        }

        // -----------------------------
        // VOLUME
        // -----------------------------
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Volume = (int)VolumeSlider.Value;
        }
    }
}