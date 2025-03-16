using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Media;

namespace WpfApp2
{
    public partial class CustomMessageBox : Window
    {
        private Action?[] buttonActions;

        public CustomMessageBox(string title, string message, string icon, string[] buttons, Action[]? actions = null)
        {
            InitializeComponent();
            this.DataContext = this;
            this.TitleText = title;
            this.Message = message;

            buttonActions = new Action[buttons.Length];
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    buttonActions[i] = actions[i];
                }
            }

            string iconData = string.Empty;
            Brush iconColor = Brushes.Transparent;
            Brush darkModeColor = Brushes.Transparent;
            Brush lightModeColor = Brushes.Transparent;
            SystemSound? systemSound = null;

            switch (icon)
            {
                case "Question":
                    iconData = "M10 2a8 8 0 1 1 0 16a8 8 0 0 1 0-16zm0 11.5a.75.75 0 1 0 0 1.5a.75.75 0 0 0 0-1.5zm0-8A2.5 2.5 0 0 0 7.5 8a.5.5 0 0 0 1 0a1.5 1.5 0 1 1 2.632.984l-.106.11l-.118.1l-.247.185a3.11 3.11 0 0 0-.356.323C9.793 10.248 9.5 10.988 9.5 12a.5.5 0 0 0 1 0c0-.758.196-1.254.535-1.614l.075-.076l.08-.073l.088-.072l.219-.163l.154-.125A2.5 2.5 0 0 0 10 5.5z";
                    darkModeColor = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // LightBlue
                    lightModeColor = new SolidColorBrush(Color.FromRgb(70, 130, 180)); // SteelBlue
                    systemSound = SystemSounds.Question;
                    break;
                case "Error":
                    iconData = "M10 2a8 8 0 1 1 0 16a8 8 0 0 1 0-16zM7.81 7.114a.5.5 0 0 0-.638.058l-.058.069a.5.5 0 0 0 .058.638L9.292 10l-2.12 2.121l-.058.07a.5.5 0 0 0 .058.637l.069.058a.5.5 0 0 0 .638-.058L10 10.708l2.121 2.12l.07.058a.5.5 0 0 0 .637-.058l.058-.069a.5.5 0 0 0-.058-.638L10.708 10l2.12-2.121l.058-.07a.5.5 0 0 0-.058-.637l-.069-.058a.5.5 0 0 0-.638.058L10 9.292l-2.121-2.12l-.07-.058z";
                    darkModeColor = new SolidColorBrush(Color.FromRgb(255, 192, 203)); // LightPink
                    lightModeColor = new SolidColorBrush(Color.FromRgb(245, 87, 98));  // Firebrick
                    systemSound = SystemSounds.Hand;
                    break;
                case "Info":
                    iconData = "M18 10a8 8 0 1 0-16 0a8 8 0 0 0 16 0zM9.508 8.91a.5.5 0 0 1 .984 0L10.5 9v4.502l-.008.09a.5.5 0 0 1-.984 0l-.008-.09V9l.008-.09zM9.25 6.75a.75.75 0 1 1 1.5 0a.75.75 0 0 1-1.5 0z";
                    darkModeColor = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // LightBlue
                    lightModeColor = new SolidColorBrush(Color.FromRgb(70, 130, 180)); // SteelBlue
                    systemSound = SystemSounds.Asterisk;
                    break;
                case "Warning":
                    iconData = "M8.686 2.852L2.127 14.777A1.5 1.5 0 0 0 3.441 17H16.56a1.5 1.5 0 0 0 1.314-2.223L11.314 2.852a1.5 1.5 0 0 0-2.628 0zM10 6.75a.75.75 0 0 1 .75.75v4a.75.75 0 0 1-1.5 0v-4a.75.75 0 0 1 .75-.75zm.75 7a.75.75 0 1 1-1.5 0a.75.75 0 0 1 1.5 0z";
                    darkModeColor = new SolidColorBrush(Color.FromRgb(255, 255, 224)); // LightYellow
                    lightModeColor = new SolidColorBrush(Color.FromRgb(218, 165, 32));  // Goldenrod
                    systemSound = SystemSounds.Exclamation;
                    break;
            }

            if (IsDarkMode())
            {
                iconColor = darkModeColor;
            }
            else
            {
                iconColor = lightModeColor;
            }

            IconPath.Data = Geometry.Parse(iconData);
            IconPath.Fill = iconColor;

            if (systemSound != null)
            {
                systemSound.Play();
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = (Button)this.FindName($"Button{i + 1}");
                if (button != null)
                {
                    button.Content = buttons[i];
                    button.Visibility = Visibility.Visible;
                    AdjustButtonWidth(button);
                }
            }

            AdjustButtonsPanel();

            double height = 200 + (message.Length / 50) * 20;
            if (height > 300)
                height = 300;
            this.Height = height;
        }

        public string TitleText { get; set; }
        public string Message { get; set; }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Maximize or minimize window on double click
                this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Minimized;
            }
            else
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int buttonIndex = int.Parse(button.Name.Replace("Button", "")) - 1;
                if (buttonActions[buttonIndex] != null)
                {
                    buttonActions[buttonIndex]?.Invoke();
                }
                this.Close(); // Close the window regardless of the button clicked
            }
        }

        private void AdjustButtonWidth(Button button)
        {
            var text = button.Content.ToString();
            var textBlock = new TextBlock { Text = text, FontFamily = button.FontFamily, FontSize = button.FontSize };
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var width = textBlock.DesiredSize.Width + 30; // 30 is for padding
            button.Width = width > 70 ? width : 70; // Ensure minimum width is 70
        }

        private bool IsDarkMode()
        {
            if (Application.Current.Resources.Contains("WindowBackgroundColor"))
            {
                var backgroundColor = (Color)Application.Current.Resources["WindowBackgroundColor"];
                var brightness = backgroundColor.R * 0.2126 + backgroundColor.G * 0.7152 + backgroundColor.B * 0.0722;
                return brightness < 128;
            }
            return false; // Default to light mode if the resource is not found
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustButtonsPanel();
        }

        private void AdjustButtonsPanel()
        {
            double totalButtonsWidth = 0;
            int visibleButtonCount = 0;
            foreach (var button in ButtonsPanel.Children)
            {
                if (button is Button btn && btn.Visibility == Visibility.Visible)
                {
                    totalButtonsWidth += btn.Width + btn.Margin.Left + btn.Margin.Right;
                    visibleButtonCount++;
                }
            }

            if (totalButtonsWidth > this.ActualWidth)
            {
                ButtonsPanel.Margin = new Thickness(10, 10, 10, 10); // Adjust margins to fit buttons
            }
            else
            {
                ButtonsPanel.Margin = new Thickness(this.ActualWidth - totalButtonsWidth - 20, 10, 10, 10); // Right-align buttons
            }
        }
    }
}