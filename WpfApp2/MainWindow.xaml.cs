using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private string settingsFilePath = "Resources/settings.settings";
        private string logFilePath = "Resources/log.txt";
        private string latestVersionUrl = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";
        private string downloadUrlTemplate = "https://github.com/yt-dlp/yt-dlp/releases/download/{0}/yt-dlp.exe";
        private HttpClient httpClient;
        private DispatcherTimer timer;
        private FileNameEditor? editor;

        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");
            LoadSettings();
            ValidateAndUpdateConfigSettings();  // ここで設定を確認・更新
            this.Closed += MainWindow_Closed;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            CheckForUpdates();
        }

        private async void CheckForUpdates()
        {
            try
            {
                var latestVersion = await GetLatestVersion();
                var currentVersion = GetCurrentVersion();

                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
                {
                    ShowCustomMessageBox("アップデート", $"新しいバージョン {latestVersion} が利用可能です。ダウンロードしますか？", "Info",
                        new string[] { "はい", "いいえ" },
                        new Action[] { async () => { await DownloadLatestVersion(latestVersion); }, () => { } });
                }
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox("エラー", $"アップデートのチェック中にエラーが発生しました: {ex.Message}", "Error", new string[] { "OK" }, new Action[] { () => { } });
            }
        }

        private async Task<string> GetLatestVersion()
        {
            var response = await httpClient.GetStringAsync(latestVersionUrl);
            var jsonDocument = JsonDocument.Parse(response);
            var latestVersion = jsonDocument.RootElement.GetProperty("tag_name").GetString();
            return latestVersion ?? string.Empty;
        }

        private string GetCurrentVersion()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "yt-dlp.exe");
            if (File.Exists(exePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                return versionInfo?.FileVersion ?? string.Empty;
            }
            return string.Empty;
        }

        private async Task DownloadLatestVersion(string latestVersion)
        {
            string downloadUrl = string.Format(downloadUrlTemplate, latestVersion);
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "yt-dlp.exe");

            using (var response = await httpClient.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();
                using (var fileStream = new FileStream(exePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            ShowCustomMessageBox("アップデート", "ダウンロードが完了しました。", "Info", new string[] { "OK" }, new Action[] { () => { } });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            SaveSettings(textbox_path.Text);
            LoadSettings();
        }

        private void SaveSettings(string path)
        {
            Directory.CreateDirectory("Resources");
            string content = "[setting]\n" +
                             "save_path=" + path + "\n" +
                             "filename=" + textbox_filename.Text + "\n" +
                             "format=" + GetSelectedFormat() + "\n" +
                             "quality=" + GetSelectedQuality();
            File.WriteAllText(settingsFilePath, content);
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                var lines = File.ReadAllLines(settingsFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("save_path="))
                    {
                        string path = line.Substring("save_path=".Length).Trim();
                        if (Directory.Exists(path))
                        {
                            textbox_path.Text = path;
                        }
                    }

                    if (line.StartsWith("filename="))
                    {
                        textbox_filename.Text = line.Substring("filename=".Length).Trim();
                    }

                    if (line.StartsWith("format="))
                    {
                        string format = line.Substring("format=".Length).Trim();
                        SetSelectedFormat(format);
                    }

                    if (line.StartsWith("quality="))
                    {
                        string quality = line.Substring("quality=".Length).Trim();
                        SetSelectedQuality(quality);
                    }
                }
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (editor != null)
            {
                editor.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textbox_url.Text = Clipboard.GetText();
            }
        }

        private void button_browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "フォルダーを選択してください"
            };

            if (dialog.ShowDialog() == true)
            {
                var folderPath = dialog.FolderName;
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    textbox_path.Text = folderPath;
                    SaveSettings(folderPath);
                }
            }
        }

        private void FormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (pannel_format.Children.Contains(radioButton))
                {
                    expander_format.Header = radioButton.Content;
                }
                else if (pannel_quality.Children.Contains(radioButton))
                {
                    expander_quality.Header = radioButton.Content;
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string url = textbox_url.Text.Trim();
            string savePath = textbox_path.Text.Trim();
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "yt-dlp.exe");

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(savePath))
            {
                ShowCustomMessageBox("エラー", "URLと保存先を指定してください。", "Error", new string[] { "OK" }, new Action[] { () => { } });
                return;
            }

            if (!File.Exists(exePath))
            {
                ShowCustomMessageBox("エラー", "yt-dlp.exe が見つかりません。", "Error", new string[] { "OK" }, new Action[] { () => { } });
                return;
            }

            string format = GetSelectedFormat();
            string quality = GetSelectedQuality();
            string filename = string.IsNullOrWhiteSpace(textbox_filename.Text) ? "%(title)s" : textbox_filename.Text.Trim();

            string arguments = CreateArguments(url, savePath, format, quality, filename);

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.OutputDataReceived += (s, ev) => ParseProgress(ev.Data);
                    process.ErrorDataReceived += (s, ev) => LogError(ev.Data);
                    StringBuilder errorOutput = new StringBuilder();
                    process.ErrorDataReceived += (s, ev) =>
                    {
                        if (!string.IsNullOrEmpty(ev.Data))
                        {
                            errorOutput.AppendLine(ev.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    textblox_downloadnow.Text = "ダウンロード中…";
                    progressbar_download.Value = 0;
                    await Task.Run(() => process.WaitForExit());

                    if (process.ExitCode == 0)
                    {
                        progressbar_download.Value = 100;
                        textblox_downloadnow.Text = "ダウンロード完了！";
                    }
                    else
                    {
                        textblox_downloadnow.Text = "ダウンロード失敗";
                        ShowCustomMessageBox("エラー", $"エラーが発生しました。\n{errorOutput.ToString()}", "Error", new string[] { "OK", "コピー" }, new Action[] { () => { }, () => Clipboard.SetText(errorOutput.ToString()) });
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void ParseProgress(string? output)
        {
            if (string.IsNullOrEmpty(output)) return;
            var match = System.Text.RegularExpressions.Regex.Match(output, "(\\d+\\.\\d+)%");
            if (match.Success && double.TryParse(match.Groups[1].Value, out double progress))
            {
                Dispatcher.Invoke(() => progressbar_download.Value = progress);
            }
        }

        private void LogError(string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"[{DateTime.Now}] エラー: {error}");
            }
        }

        private void ShowCustomMessageBox(string title, string message, string icon, string[] buttons, Action[] actions)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox(title, message, icon, buttons, actions);
            customMessageBox.Owner = this; // 親ウィンドウを指定
            customMessageBox.ShowDialog();
        }

        private string GetSelectedFormat()
        {
            if (radiobutton_mp3.IsChecked == true)
                return "mp3";
            else if (radiobutton_m4a.IsChecked == true)
                return "m4a";
            else if (radiobutton_wav.IsChecked == true)
                return "wav";
            else if (radiobutton_jpg.IsChecked == true)
                return "thumbnail";
            return "mp4";
        }

        private void SetSelectedFormat(string format)
        {
            switch (format)
            {
                case "mp3":
                    radiobutton_mp3.IsChecked = true;
                    break;
                case "m4a":
                    radiobutton_m4a.IsChecked = true;
                    break;
                case "wav":
                    radiobutton_wav.IsChecked = true;
                    break;
                case "thumbnail":
                    radiobutton_jpg.IsChecked = true;
                    break;
                default:
                    radiobutton_mp4.IsChecked = true;
                    break;
            }
        }

        private string GetSelectedQuality()
        {
            if (radiobutton_4320.IsChecked == true)
                return "4320";
            else if (radiobutton_2160.IsChecked == true)
                return "2160";
            else if (radiobutton_1440.IsChecked == true)
                return "1440";
            else if (radiobutton_1080.IsChecked == true)
                return "1080";
            else if (radiobutton_720.IsChecked == true)
                return "720";
            else if (radiobutton_480.IsChecked == true)
                return "480";
            else if (radiobutton_360.IsChecked == true)
                return "360";
            else if (radiobutton_240.IsChecked == true)
                return "240";
            else if (radiobutton_144.IsChecked == true)
                return "144";
            return "最高画質";
        }

        private void SetSelectedQuality(string quality)
        {
            switch (quality)
            {
                case "4320":
                    radiobutton_4320.IsChecked = true;
                    break;
                case "2160":
                    radiobutton_2160.IsChecked = true;
                    break;
                case "1440":
                    radiobutton_1440.IsChecked = true;
                    break;
                case "1080":
                    radiobutton_1080.IsChecked = true;
                    break;
                case "720":
                    radiobutton_720.IsChecked = true;
                    break;
                case "480":
                    radiobutton_480.IsChecked = true;
                    break;
                case "360":
                    radiobutton_360.IsChecked = true;
                    break;
                case "240":
                    radiobutton_240.IsChecked = true;
                    break;
                case "144":
                    radiobutton_144.IsChecked = true;
                    break;
                default:
                    radiobutton_hight.IsChecked = true;
                    break;
            }
        }

        private string CreateArguments(string url, string savePath, string format, string quality, string filename)
        {
            var configSettings = LoadConfigSettings();
            string cookiesfrombrowser = configSettings.ContainsKey("cookiesfrombrowser") ? configSettings["cookiesfrombrowser"].ToLower() : string.Empty;
            bool embedthumbnail = configSettings.ContainsKey("embedthumbnail") && bool.Parse(configSettings["embedthumbnail"]);
            bool embedmetadata = configSettings.ContainsKey("embedmetadata") && bool.Parse(configSettings["embedmetadata"]);
            bool mtime = configSettings.ContainsKey("mtime") && bool.Parse(configSettings["mtime"]);

            StringBuilder arguments = new StringBuilder("--progress --newline ");

            if (!string.IsNullOrEmpty(cookiesfrombrowser) && cookiesfrombrowser != "none")
            {
                if (cookiesfrombrowser == "chrome")
                {
                    try
                    {
                        arguments.Append($"--cookies-from-browser {cookiesfrombrowser} ");
                    }
                    catch (Exception ex)
                    {
                        ShowCustomMessageBox("エラー", $"Chromeのクッキーデータベースのコピーに失敗しました。エラー: {ex.Message}\n詳細はhttps://github.com/yt-dlp/yt-dlp/issues/7271 を参照してください。", "Error", new string[] { "OK" }, new Action[] { () => { } });
                        // クッキー設定をスキップ
                    }
                }
                else
                {
                    arguments.Append($"--cookies-from-browser {cookiesfrombrowser} ");
                }
            }

            if (embedthumbnail)
            {
                arguments.Append("--embed-thumbnail ");
            }
            if (embedmetadata)
            {
                arguments.Append("--embed-metadata ");
            }
            if (!mtime)
            {
                arguments.Append("--no-mtime ");
            }

            string formatTextMp4;
            if (quality == "最高画質")
            {
                formatTextMp4 = "bestvideo[ext=mp4]+bestaudio[ext=m4a]/bestvideo[ext=mp4]+bestaudio/best[ext=mp4]";
            }
            else
            {
                formatTextMp4 = $"bestvideo[ext=mp4][height<={quality}]+bestaudio[ext=m4a]/bestvideo[ext=mp4][height<={quality}]+bestaudio/best[ext=mp4]";
            }

            if (format == "mp4")
            {
                arguments.Append($"-f \"{formatTextMp4}\" \"{url}\" -o \"{Path.Combine(savePath, filename)}\"");
            }
            else if (format == "mp3" || format == "m4a")
            {
                arguments.Append($"--extract-audio --audio-format {format} \"{url}\" -o \"{Path.Combine(savePath, filename)}\"");
            }
            else if (format == "wav")
            {
                arguments.Append($"--extract-audio --audio-format wav \"{url}\" -o \"{Path.Combine(savePath, filename)}\"");
            }
            else
            {
                arguments.Append($"--write-thumbnail --convert-thumbnail jpg \"{url}\" -o \"{Path.Combine(savePath, filename)}\"");
            }

            return arguments.ToString();
        }

        private void button_setting_Click(object sender, RoutedEventArgs e)
        {
            var setting = new SettingWindow();
            var ret = setting.ShowDialog();
        }

        private void button_edit_Click(object sender, RoutedEventArgs e)
        {
            if (editor == null || !editor.IsVisible)
            {
                editor = new FileNameEditor();
                editor.Show();
            }
            else
            {
                editor.Activate();
            }
        }

        private Dictionary<string, string> LoadConfigSettings()
        {
            var configSettings = new Dictionary<string, string>();
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.settings");

            if (File.Exists(configFilePath))
            {
                var lines = File.ReadAllLines(configFilePath);
                foreach (var line in lines)
                {
                    if (line.Contains('='))
                    {
                        var parts = line.Split('=', 2);
                        var key = parts[0].Trim();
                        var value = parts[1].Trim().Trim('\''); // 値の前後のシングルクォートを削除
                        configSettings[key] = value;
                    }
                }
            }

            return configSettings;
        }

        private void ValidateAndUpdateConfigSettings()
        {
            var configSettings = LoadConfigSettings();
            string[] supportedBrowsers = { "brave", "chrome", "chromium", "edge", "firefox", "opera", "safari", "vivaldi", "whale" };

            if (configSettings.ContainsKey("cookiesfrombrowser"))
            {
                string browser = configSettings["cookiesfrombrowser"].ToLower();
                if (!supportedBrowsers.Contains(browser) && browser != "none")
                {
                    // サポートされていないブラウザなので、chromeに変更
                    configSettings["cookiesfrombrowser"] = "chrome";

                    // 更新された設定を書き戻す
                    SaveConfigSettings(configSettings);
                }
            }
        }

        private void SaveConfigSettings(Dictionary<string, string> configSettings)
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.settings");
            List<string> lines = new List<string>();

            foreach (var kvp in configSettings)
            {
                lines.Add($"{kvp.Key}='{kvp.Value}'");
            }

            File.WriteAllLines(configFilePath, lines);
        }
    }
}