using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameruImagesUploader
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            UpdateUIFromSettings();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                Properties.Settings.Default.Reload();
            }
        }

        // Event handlers

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SetLanguage();
            RegisterInStartup(CheckBoxAutoRun.IsChecked ?? false);
            RegisterInContextmenu(CheckBoxContextMenu.IsChecked ?? false);

            var newValue = CheckBoxMakeScreenshots.IsChecked ?? false;
            App.notifyIcon.ContextMenu.MenuItems[1].Checked = newValue;
            Properties.Settings.Default.MakeScreenshots = newValue;
            Properties.Settings.Default.Save();
        }

        private void ComboBoxImagesPerLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int value = Convert.ToInt32(((ComboBoxItem)e.AddedItems[0]).Content);
                Properties.Settings.Default.ImagesPerLine = value;
            }
            catch { }
        }

        private void ComboBoxСolorScheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    //Properties.Settings.Default.Reload();
                    break;
                case 1:
                    Properties.Settings.Default.ColorTopWideRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB21301"));
                    Properties.Settings.Default.ColorTopNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF910F00"));
                    Properties.Settings.Default.ColorMiddleRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorBottomNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF910F00"));
                    Properties.Settings.Default.ColorProgressBarBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorProgressBarForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF06B025"));
                    Properties.Settings.Default.ColorListBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorTextBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorTextBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                    Properties.Settings.Default.ColorLabelForegroung = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                    Properties.Settings.Default.ColorLabelStatusBar = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB21301"));
                    Properties.Settings.Default.ColorButtonBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD01600"));
                    Properties.Settings.Default.ColorComboBoxItemBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF910F00"));
                    Properties.Settings.Default.ColorComboBoxItemBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD01600"));
                    Properties.Settings.Default.ColorComboBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                    Properties.Settings.Default.ColorComboBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB21301"));
                    Properties.Settings.Default.ColorComboBoxBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD01600"));
                    Properties.Settings.Default.LogoVisibility = true;
                    Properties.Settings.Default.СolorScheme = Properties.Resources.СolorSchemeDefault;
                    break;
                case 2:
                    Properties.Settings.Default.ColorTopWideRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8C8D2"));
                    Properties.Settings.Default.ColorTopNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEF2"));
                    Properties.Settings.Default.ColorMiddleRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEF2"));
                    Properties.Settings.Default.ColorBottomNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
                    Properties.Settings.Default.ColorProgressBarBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEF2"));
                    Properties.Settings.Default.ColorProgressBarForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9DEF5"));
                    Properties.Settings.Default.ColorListBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorTextBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorTextBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                    Properties.Settings.Default.ColorLabelForegroung = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                    Properties.Settings.Default.ColorLabelStatusBar = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8C8D2"));
                    Properties.Settings.Default.ColorButtonBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9DEF5"));
                    Properties.Settings.Default.ColorComboBoxItemBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F6F6"));
                    Properties.Settings.Default.ColorComboBoxItemBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9DEF5"));
                    Properties.Settings.Default.ColorComboBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                    Properties.Settings.Default.ColorComboBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F6F6"));
                    Properties.Settings.Default.ColorComboBoxBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9DEF5"));
                    Properties.Settings.Default.LogoVisibility = false;
                    Properties.Settings.Default.СolorScheme = Properties.Resources.СolorSchemeLight;
                    break;
                case 3:
                    Properties.Settings.Default.ColorTopWideRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                    Properties.Settings.Default.ColorTopNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333337"));
                    Properties.Settings.Default.ColorMiddleRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                    Properties.Settings.Default.ColorBottomNarrowRow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
                    Properties.Settings.Default.ColorProgressBarBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333337"));
                    Properties.Settings.Default.ColorProgressBarForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#148CDC"));
                    Properties.Settings.Default.ColorListBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                    Properties.Settings.Default.ColorTextBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                    Properties.Settings.Default.ColorTextBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorLabelForegroung = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorLabelStatusBar = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorButtonBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                    Properties.Settings.Default.ColorButtonBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
                    Properties.Settings.Default.ColorComboBoxItemBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B1B1C"));
                    Properties.Settings.Default.ColorComboBoxItemBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
                    Properties.Settings.Default.ColorComboBoxForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    Properties.Settings.Default.ColorComboBoxBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30"));
                    Properties.Settings.Default.ColorComboBoxBackgroundHighlighted = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
                    Properties.Settings.Default.LogoVisibility = false;
                    Properties.Settings.Default.СolorScheme = Properties.Resources.СolorSchemeDark;
                    break;
            }
        }

        private void ButtonColor_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.СolorScheme = Properties.Resources.СolorSchemeModified;
            ComboBoxСolorScheme.SelectedIndex = 0;

            var colorPickerWindows = new ColorPickerWindow();
            colorPickerWindows.Owner = this;
            SolidColorBrush brush = ((Button)sender).Background as SolidColorBrush;
            colorPickerWindows.UpdateColor(brush.Color);
            bool result = colorPickerWindows.ShowDialog() ?? false;
            if (result)
            {
                SolidColorBrush brushResultColor = new SolidColorBrush();
                brushResultColor.Color = colorPickerWindows.color;
                ((Button)sender).Background = brushResultColor;
            }
        }

        private void ButtonLoginLogout_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.IsLogin() == false)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Owner = this;
                loginWindow.ShowDialog();
                if (MainWindow.IsLogin())
                {
                    TextBlockUserName.Text = Properties.Settings.Default.UserName;
                    ButtonLoginLogout.Content = Properties.Resources.Logout;
                }
            }
            else
            {
                Properties.Settings.Default.UserID = String.Empty;
                Properties.Settings.Default.PasswordHash = String.Empty;
                Properties.Settings.Default.AvatarPath = "Resources/ImageDefaultAvatar.png";
                Properties.Settings.Default.Save();
                TextBlockUserName.Text = Properties.Resources.Guest;
                ButtonLoginLogout.Content = Properties.Resources.Login;
            }
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Properties.Resources.MessageResetSettings, Properties.Resources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Reset();
                UpdateUIFromSettings();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.OriginalString);
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        // Functions

        private void UpdateUIFromSettings()
        {
            // Language
            switch (Properties.Settings.Default.Language)
            {
                case "en-US":
                    ComboBoxLanguage.SelectedIndex = 1;
                    break;
                case "ru-RU":
                    ComboBoxLanguage.SelectedIndex = 2;
                    break;
                default:
                    ComboBoxLanguage.SelectedIndex = 0;
                    break;
            }

            // Сolor scheme
            string colorScheme = Properties.Settings.Default.СolorScheme;
            if (colorScheme == Properties.Resources.СolorSchemeDefault)
            {
                ComboBoxСolorScheme.SelectedIndex = 1;
            }
            else if (colorScheme == Properties.Resources.СolorSchemeLight)
            {
                ComboBoxСolorScheme.SelectedIndex = 2;
            }
            else if (colorScheme == Properties.Resources.СolorSchemeDark)
            {
                ComboBoxСolorScheme.SelectedIndex = 3;
            }
            else if (colorScheme == Properties.Resources.СolorSchemeModified)
            {
                ComboBoxСolorScheme.SelectedIndex = 0;
            }
            else ComboBoxСolorScheme.SelectedIndex = 1;

            // Images per line
            try
            {
                if (Properties.Settings.Default.ImagesPerLine - 1 >= 0 &&
                    Properties.Settings.Default.ImagesPerLine - 1 < ComboBoxImagesPerLine.Items.Count)
                    ComboBoxImagesPerLine.SelectedIndex = Properties.Settings.Default.ImagesPerLine - 1;
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }

            // UserName
            if (MainWindow.IsLogin() == true)
            {
                TextBlockUserName.Text = Properties.Settings.Default.UserName;
                ButtonLoginLogout.Content = Properties.Resources.Logout;
            }
            else
            {
                TextBlockUserName.Text = Properties.Resources.Guest;
                ButtonLoginLogout.Content = Properties.Resources.Login;
            }
        }

        private void SetLanguage()
        {
            string previosLanguage = Properties.Settings.Default.Language;
            switch (ComboBoxLanguage.SelectedIndex)
            {
                case 0:
                    Properties.Settings.Default.Language = String.Empty;
                    break;
                case 1:
                    Properties.Settings.Default.Language = "en-US";
                    break;
                case 2:
                    Properties.Settings.Default.Language = "ru-RU";
                    break;
            }
            if (previosLanguage != Properties.Settings.Default.Language)
            {
                var result = MessageBox.Show(Properties.Resources.LanguageInformation, 
                    Properties.Resources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    App.mutex.Dispose();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }
        }

        private void RegisterInStartup(bool isEnable)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string keyName = Assembly.GetExecutingAssembly().FullName;
                string keyValue = String.Format("\"{0}\" {1}", Assembly.GetExecutingAssembly().Location, Properties.Resources.LaunchKeyHide);
                if (isEnable)
                {
                    registryKey.SetValue(keyName, keyValue);
                }
                else
                {
                    if (registryKey.GetValue(keyName) != null)
                    {
                        registryKey.DeleteValue(Assembly.GetExecutingAssembly().FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void RegisterInContextmenu(bool isEnable)
        {
            string shellKeyName = Assembly.GetExecutingAssembly().GetName().Name;
            if (isEnable)
            {
                string menuCommand = string.Format("\"{0}\" \"%1\" {1}", Assembly.GetExecutingAssembly().Location, Properties.Resources.LaunchKeyUpload);
                string iconPath = Assembly.GetExecutingAssembly().Location + ",0";
                FileShellExtension.Register("giffile", shellKeyName, Properties.Resources.ContextMenuTitle, menuCommand, iconPath);
                FileShellExtension.Register("jpegfile", shellKeyName, Properties.Resources.ContextMenuTitle, menuCommand, iconPath);
                FileShellExtension.Register("pngfile", shellKeyName, Properties.Resources.ContextMenuTitle, menuCommand, iconPath);
            }
            else
            {
                FileShellExtension.Unregister("giffile", shellKeyName);
                FileShellExtension.Unregister("jpegfile", shellKeyName);
                FileShellExtension.Unregister("pngfile", shellKeyName);
            }
        }
    }

    static class FileShellExtension
    {
        public static void Register(string fileType, string shellKeyName, string menuText, string menuCommand, string iconPath)
        {
            try
            {
                // Create path to registry location
                string regPath = String.Format(@"Software\Classes\{0}\shell\{1}", fileType, shellKeyName);

                // Add context menu to the registry
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath))
                {
                    key.SetValue(null, menuText);
                    key.SetValue("Icon", iconPath);
                }

                // Add command that is invoked to the registry
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(string.Format(@"{0}\command", regPath)))
                {
                    key.SetValue(null, menuCommand);
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        public static void Unregister(string fileType, string shellKeyName)
        {
            try
            {
                // Path to the registry location
                string regPath = string.Format(@"Software\Classes\{0}\shell\{1}", fileType, shellKeyName);

                // Remove context menu from the registry
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(regPath, true);
                if (registryKey != null)
                {
                    if (registryKey.GetValue("Icon") != null)
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(regPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }
    }
}