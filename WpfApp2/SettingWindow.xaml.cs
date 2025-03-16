using System;
using System.IO;
using System.Windows;

namespace WpfApp2
{
    public partial class SettingWindow : Window
    {
        private bool _isSaved = false;

        public SettingWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            string resourceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            string settingsFilePath = Path.Combine(resourceFolderPath, "config.settings");

            if (File.Exists(settingsFilePath))
            {
                string[] settings = File.ReadAllLines(settingsFilePath);
                foreach (string setting in settings)
                {
                    string[] keyValue = setting.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();

                        switch (key)
                        {
                            case "cookiesfrombrowser":
                                switch (value)
                                {
                                    case "chrome": CookieChrome.IsChecked = true; break;
                                    case "edge": CookieEdge.IsChecked = true; break;
                                    case "firefox": CookieFireFox.IsChecked = true; break;
                                    case "brave": CookieBrave.IsChecked = true; break;
                                    case "vivaldi": CookieVivaldi.IsChecked = true; break;
                                    case "opera": CookieOpera.IsChecked = true; break;
                                    case "none": CookieNone.IsChecked = true; break;
                                }
                                break;
                            case "embedthumbnail":
                                EmbedThumbnailCheckBox.IsChecked = value == "True";
                                break;
                            case "embedmetadata":
                                EmbedMetadataCheckBox.IsChecked = value == "True";
                                break;
                            case "mtime":
                                TimeNomtime.IsChecked = value == "False";
                                TimeMtime.IsChecked = value == "True";
                                break;
                            case "nolog":
                                NoLogCheckBox.IsChecked = value == "True";
                                break;
                        }
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCookie = "none";
            if (CookieChrome.IsChecked == true) selectedCookie = "chrome";
            else if (CookieEdge.IsChecked == true) selectedCookie = "edge";
            else if (CookieFireFox.IsChecked == true) selectedCookie = "firefox";
            else if (CookieBrave.IsChecked == true) selectedCookie = "brave";
            else if (CookieVivaldi.IsChecked == true) selectedCookie = "vivaldi";
            else if (CookieOpera.IsChecked == true) selectedCookie = "opera";
            else if (CookieNone.IsChecked == true) selectedCookie = "none";

            bool embedThumbnail = EmbedThumbnailCheckBox.IsChecked == true;
            bool embedMetadata = EmbedMetadataCheckBox.IsChecked == true;
            bool applyNomtime = TimeNomtime.IsChecked == true;
            bool noLog = NoLogCheckBox.IsChecked == true;

            var settings = "[config]\n" +
                           $"cookiesfrombrowser={selectedCookie}\n" +
                           $"embedthumbnail={embedThumbnail}\n" +
                           $"embedmetadata={embedMetadata}\n" +
                           $"mtime={!applyNomtime}\n" +
                           $"nolog={noLog}";

            string resourceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(resourceFolderPath))
            {
                Directory.CreateDirectory(resourceFolderPath);
            }
            string settingsFilePath = Path.Combine(resourceFolderPath, "config.settings");

            try
            {
                File.WriteAllText(settingsFilePath, settings);
                _isSaved = true;
                this.Close();
            }
            catch (Exception)//必要な場合ex
            {
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isSaved)
            {
                ShowCustomMessageBox("警告", "保存してない変更がありますが終了しても\nよろしいですか？", "Warning", new string[] { "はい", "いいえ" }, new Action[] { () => { }, () => { e.Cancel = true; } });
            }
        }

        private void ShowCustomMessageBox(string title, string message, string icon, string[] buttons, Action[] actions)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox(title, message, icon, buttons, actions);
            customMessageBox.Owner = this; // 親ウィンドウを指定
            customMessageBox.ShowDialog();
        }
    }
}