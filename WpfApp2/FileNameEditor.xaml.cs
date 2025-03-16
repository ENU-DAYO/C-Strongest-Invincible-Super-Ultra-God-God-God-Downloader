using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class FileNameEditor : Window
    {
        private DispatcherTimer dispatcherTimer;

        public FileNameEditor()
        {
            InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(0); // 0秒ごとに更新
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                UpdateTextBlock(mainWindow.textbox_filename.Text);
            }
        }

        private void OnAddField(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string fieldTag)
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.textbox_filename.Text += $"%({fieldTag})s";
                }
            }
        }

        private void UpdateTextBlock(string text)
        {
            textBlock.Text = text
                .Replace("%(id)s", "動画ID")
                .Replace("%(title)s", "タイトル")
                .Replace("%(description)s", "概要")
                .Replace("%(playlist_title)s", "プレイリストの名前")
                .Replace("%(playlist_index)s", "プレリストの番号")
                .Replace("%(ext)s", "拡張子")
                .Replace("%(fps)s", "FPS")
                .Replace("%(width)s", "動画の幅")
                .Replace("%(height)s", "動画の高さ")
                .Replace("%(webpage_url_domain)s", "サイト名")
                .Replace("%(upload_date)s", "投稿日")
                .Replace("%(uploader)s", "投稿者")
                .Replace("%(uploader_id)s", "投稿者のID");

            if (textBlock.ActualHeight > this.ActualHeight)
            {
                this.Height = textBlock.ActualHeight + 100; // 余白を追加
            }
        }
    }
}