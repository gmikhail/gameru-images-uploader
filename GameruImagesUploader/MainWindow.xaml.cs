using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace GameruImagesUploader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] imageLinks = null;// Contains links to already uploaded images

        public MainWindow()
        {
            SetCurrentUICulture();
            InitializeComponent();
            InitializeKeyboardListener();
            Log.Clear();
            Log.Add(Assembly.GetExecutingAssembly().GetName().FullName);
        }

        // Windows

        public new void Show()
        {
            base.Show();
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (Properties.Settings.Default.NotificationAreaMinimize == true)
                {
                    Hide();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.NotificationAreaClose == true)
            {
                Hide();
                e.Cancel = true;
            }
        }

        // Drag & Drop

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null) return;
            foreach (var filePath in filePaths)
            {
                try
                {
                    string extension = Path.GetExtension(filePath).Remove(0, 1);
                    if (!(extension == "gif" ||
                        extension == "jpg" ||
                        extension == "jpeg" ||
                        extension == "png"))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                AddImages(filePaths);
        }

        // Keyboard Hook

        internal KeyboardHook keyboardListener;

        private void InitializeKeyboardListener()
        {
            keyboardListener = new KeyboardHook();
            keyboardListener.OnKeyEventHandler += KeyboardListener_OnKeyEventHandler;
            keyboardListener.HookKeyboard();
        }

        bool makeScreenshot = true;
        Key previousKey = Key.None;

        private void KeyboardListener_OnKeyEventHandler(object sender, KeyEvent e)
        {
            if (Properties.Settings.Default.MakeScreenshots == false) return;
            if (e.KeyDown == Key.PrintScreen && makeScreenshot == true)
            {
                if (previousKey == Key.LeftAlt || previousKey == Key.RightAlt)
                {
                    var image = MakeScreenshotFromForegroundWindow();
                    string filePath = SaveScreenshot(image);
                    if (filePath != null) AddImage(filePath);
                }
                else
                {
                    var image = MakeScreenshotFromAllScreen();
                    string filePath = SaveScreenshot(image);
                    if (filePath != null) AddImage(filePath);
                }
                makeScreenshot = false;
            }
            if ((e.KeyUp == Key.LeftAlt || e.KeyUp == Key.RightAlt) ||
                e.KeyUp == Key.PrintScreen) makeScreenshot = true;
            previousKey = e.KeyDown;
        }

        // Screenshots

        private string SaveScreenshot(BitmapImage bitmapImage)
        {
            try
            {
                int count = 0;
                string screenshotsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + '\\' + Properties.Resources.Screenshots;
                var format = ImageFormat.Png;
                while (File.Exists(screenshotsDirectory + '\\' + String.Format("{0} ({1}).{2}",
                    Properties.Resources.Screenshot, count, format.ToString().ToLower()))) count++;
                string fileName = String.Format("{0} ({1}).{2}",
                    Properties.Resources.Screenshot, count, format.ToString().ToLower());
                string filePath = screenshotsDirectory + '\\' + fileName;
                return SaveBitmapImage(bitmapImage, filePath, format);
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return null;
        }

        private BitmapImage MakeScreenshotFromForegroundWindow()
        {
            var rect = ForegroundWindow.GetForegroundWindowRect();
            var rec = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);
            return GetBitmapImageFromScreen(rec, ImageFormat.Png);
        }

        private BitmapImage MakeScreenshotFromAllScreen()
        {
            var rect = new System.Drawing.Rectangle(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            return GetBitmapImageFromScreen(rect, ImageFormat.Png);
        }

        public static BitmapImage GetBitmapImageFromScreen(System.Drawing.Rectangle rect, ImageFormat format)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var screen = System.Windows.Forms.Screen.FromRectangle(rect);
                    var bitmap = new System.Drawing.Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var graphics = System.Drawing.Graphics.FromImage(bitmap);
                    graphics.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, System.Drawing.CopyPixelOperation.SourceCopy);
                    bitmap.Save(ms, format);
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = ms;
                    img.EndInit();
                    return img;
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
            return null;
        }

        // Event handlers

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (IsLogin() == false)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Owner = this;
                loginWindow.ShowDialog();
            }
            else
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.TabControl.SelectedIndex = 2;
                settingsWindow.ShowDialog();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = true;
            StringBuilder sb = new StringBuilder();
            sb.Append(Properties.Resources.AllTypes + ' ');
            sb.Append("(*.gif, *.jpg, *.jpeg, *.png)|*.gif;*.jpg;*.jpeg;*.png");
            sb.Append("|GIF (*.gif)|*.gif");
            sb.Append("|JPEG (*.jpg, *.jpeg)|*.jpg;*.jpeg");
            sb.Append("|PNG (*.png)|*.png");
            ofd.Filter = sb.ToString();
            var result = ofd.ShowDialog();
            if (result == true)
            {
                AddImages(ofd.FileNames);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedImages();
        }

        private void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            UploadAllImages();
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TextBoxLinks.Text);
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void ComboBoxLinksType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLinks();
        }

        private void ListBoxImages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ListBox)sender).SelectedItems.Count != 1) return;
            var source = ((Image)((ListBox)sender).SelectedItem).Source.ToString();
            try
            {
                Process.Start(source);
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void ListBoxImages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelectedImages();
            }
        }

        // Images & Uploading

        private byte[] serverResponse = null;// Server response (containts links or error message)
        internal bool isUploading = false;// Uploading now or not 
        internal int uploadedImagesCount = 0;// Already uploaded images
        private string tempPath = Path.GetTempPath() + Assembly.GetEntryAssembly().GetName().Name;

        internal void AddImage(string filePath)
        {
            AddImages(new string[] { filePath });
        }

        internal void AddImages(string[] filePaths)
        {
            bool imageSizeLimit = false;// Display a message on the size limit only once
            foreach (string filePath in filePaths)
            {
                try
                {
                    var image = new Image();
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.DecodePixelWidth = 100;
                    bitmapImage.UriSource = new Uri(filePath);
                    bitmapImage.EndInit();
                    image.Source = bitmapImage;
                    image.Tag = filePath;// Save file path

                    int maximumImageSize = 3000000;// Maximum file size in bytes
                    if (IsLogin() == false && new FileInfo(filePath).Length > maximumImageSize)
                    {
                        if (!imageSizeLimit)
                        {
                            imageSizeLimit = true;
                            var result = MessageBox.Show(Properties.Resources.MessageImageSizeLimit, Properties.Resources.Error,
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Yes)
                            {
                                var loginWindow = new LoginWindow();
                                if (Visibility == System.Windows.Visibility.Visible) loginWindow.Owner = this;
                                else loginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                loginWindow.ShowDialog();
                                if (IsLogin()) ListBoxImages.Items.Add(image);
                            }
                        }
                        continue;
                    }
                    ListBoxImages.Items.Add(image);
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }
            }
            UpdateFilesInformation();

            if (Properties.Settings.Default.UploadImagesAutomatically == true)
            {
                if (isUploading == false) UploadAllImages();
            }
        }

        internal async void UploadAllImages()
        {
            if (ListBoxImages.Items.Count == 0) return;
            try
            {
                BeforeUpload();

                // Add authorization data
                NameValueCollection valueCollection = null;
                if (IsLogin())
                {
                    valueCollection = new NameValueCollection();
                    var cookieData = new StringBuilder();
                    cookieData.Append(Properties.Resources.CookieNameUserId + '=' + Properties.Settings.Default.UserID + ';');
                    cookieData.Append(Properties.Resources.CookieNamePasswordHash + '=' + Properties.Settings.Default.PasswordHash);
                    valueCollection.Add("Cookie", cookieData.ToString());
                }

                for (int i = 0; i < ListBoxImages.Items.Count; i++)
                {
                    BeforeOneImageUpload();

                    // Modify image (if necessary)
                    string filePath = null;
                    if (ComboBoxResize.SelectedIndex == 0 &&
                        ComboBoxRotation.SelectedIndex == 0)
                    {
                        filePath = (((Image)ListBoxImages.Items[i]).Source as BitmapImage).UriSource.LocalPath;
                    }
                    else
                    {
                        filePath = ModifyImage(ListBoxImages.Items[i] as Image);
                    }
                    if (filePath != null)
                    {
                        serverResponse = await UploadImage(Properties.Resources.UploadUrl, filePath, valueCollection);
                    }

                    AfterOneImageUpload();

                    // Last image
                    if (ListBoxImages.Items[i] == ListBoxImages.Items[ListBoxImages.Items.Count - 1])
                    {
                        AfterUpload();
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
        }

        private async Task<byte[]> UploadImage(string url, string filePath, NameValueCollection valueCollection = null)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.UploadProgressChanged += WebClientUploadProgressChanged;
                    webClient.UploadFileCompleted += WebClientUploadCompleted;
                    if (valueCollection != null) webClient.Headers.Add(valueCollection);
                    if (File.Exists(filePath))
                    {
                        string fileType = Path.GetExtension(filePath).Remove(0, 1).ToLower();
                        if (fileType == "jpg") fileType = "jpeg";
                        webClient.Headers.Add(HttpRequestHeader.ContentType, "image/" + fileType);
                        return await webClient.UploadFileTaskAsync(new Uri(url), filePath);
                    }
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                    MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        private void BeforeUpload()
        {
            isUploading = true;
            uploadedImagesCount = 0;
            serverResponse = null;
            ProgressBarAnimation(ProgressBarUpload.Minimum, TimeSpan.FromMilliseconds(0));
            ButtonSettings.IsEnabled = false;
            ButtonLogin.IsEnabled = false;
            ButtonAdd.IsEnabled = false;
            ButtonDelete.IsEnabled = false;
            ButtonUpload.IsEnabled = false;
            ComboBoxResize.IsEnabled = false;
            ComboBoxRotation.IsEnabled = false;
            App.notifyIcon.ContextMenu.MenuItems[2].Enabled = false;
            DeleteLinks();
        }

        private void BeforeOneImageUpload()
        {
            try
            {
                var filePath = (((Image)ListBoxImages.Items[uploadedImagesCount]).Source as BitmapImage).UriSource.LocalPath;
                string fileName = Path.GetFileName(filePath);
                int maxFileNameLength = 50;
                if (fileName.Length > maxFileNameLength)
                {
                    fileName = fileName.Remove(maxFileNameLength) + "...";
                }
                string information = String.Format("{0} \"{1}\"...", Properties.Resources.LabelStatusBarUploading, fileName);
                LabelStatusBar.Content = information;
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void AfterOneImageUpload()
        {
            uploadedImagesCount++;
            try
            {
                if (serverResponse != null)
                {
                    GetLinksFromServerResponse(Encoding.Default.GetString(serverResponse));
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            UpdateLinks();
        }

        private void AfterUpload()
        {
            isUploading = false;
            ButtonSettings.IsEnabled = true;
            ButtonLogin.IsEnabled = true;
            ButtonAdd.IsEnabled = true;
            ButtonDelete.IsEnabled = true;
            ButtonUpload.IsEnabled = true;
            ComboBoxResize.IsEnabled = true;
            ComboBoxRotation.IsEnabled = true;
            App.notifyIcon.ContextMenu.MenuItems[2].Enabled = true;
            try
            {
                for (int i = 0; i < ListBoxImages.Items.Count; i++)
                {
                    ((Image)ListBoxImages.Items[i]).Source = null;// For unlocking file
                }
                UpdateLayout();
                GC.Collect();// For unlocking file
                GC.WaitForPendingFinalizers();
                ListBoxImages.Items.Clear();
                UpdateFilesInformation();

                if (Properties.Settings.Default.CopyLinksAutomatically == true)
                {
                    Clipboard.SetText(TextBoxLinks.Text);
                }
                if (Properties.Settings.Default.CopyLinksAutomatically == true)
                {
                    if (imageLinks != null)
                    {
                        App.notifyIcon.ShowBalloonTip(2000, Properties.Resources.NotifyImagesUploaded,
                            Properties.Resources.NotifyLinksCopy, System.Windows.Forms.ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            DeleteTemporaryFiles();
        }

        private string ModifyImage(Image image)
        {
            string filePath = (image.Source as BitmapImage).UriSource.LocalPath;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(filePath);
            #region Resize
            string resize = ((ComboBoxItem)ComboBoxResize.SelectedItem).Content.ToString();
            if (resize == Properties.Resources.Size64)
            {
                bitmapImage.DecodePixelWidth = 64;
            }
            else if (resize == Properties.Resources.Size150)
            {
                bitmapImage.DecodePixelWidth = 150;
            }
            else if (resize == Properties.Resources.Size320)
            {
                bitmapImage.DecodePixelWidth = 320;
            }
            else if (resize == Properties.Resources.Size500)
            {
                bitmapImage.DecodePixelWidth = 500;
            }
            else if (resize == Properties.Resources.Size640)
            {
                bitmapImage.DecodePixelWidth = 640;
            }
            else if (resize == Properties.Resources.Size800)
            {
                bitmapImage.DecodePixelWidth = 800;
            }
            else if (resize == Properties.Resources.Size1024)
            {
                bitmapImage.DecodePixelWidth = 1024;
            }
            else if (resize == Properties.Resources.Size1280)
            {
                bitmapImage.DecodePixelWidth = 1280;
            }
            else if (resize == Properties.Resources.Size1600)
            {
                bitmapImage.DecodePixelWidth = 1600;
            }
            else if (resize == Properties.Resources.Size1080)
            {
                bitmapImage.DecodePixelWidth = 1920;
            }
            #endregion
            #region Rotate
            string rotate = ((ComboBoxItem)ComboBoxRotation.SelectedItem).Content.ToString();
            if (rotate == Properties.Resources.Rotate90)
            {
                bitmapImage.Rotation = Rotation.Rotate90;
            }
            else if (rotate == Properties.Resources.Rotate180)
            {
                bitmapImage.Rotation = Rotation.Rotate180;
            }
            else if (rotate == Properties.Resources.Rotate270)
            {
                bitmapImage.Rotation = Rotation.Rotate270;
            }
            #endregion
            bitmapImage.EndInit();

            string path = tempPath + "\\" + Path.GetFileName(filePath);
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            ImageFormat format;
            string extension = Path.GetExtension(filePath).Remove(0, 1);
            if (extension == ImageFormat.Gif.ToString().ToLower()) format = ImageFormat.Gif;
            else if (extension == ImageFormat.Jpeg.ToString().ToLower() || extension == "jpg") format = ImageFormat.Jpeg;
            else if (extension == ImageFormat.Png.ToString().ToLower()) format = ImageFormat.Png;
            else format = ImageFormat.Png;// Default
            return SaveBitmapImage(bitmapImage, path, format);
        }

        private string SaveBitmapImage(BitmapImage bitmapImage, string filePath, ImageFormat format)
        {
            try
            {
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, ImageToByte(bitmapImage, format));
                return filePath;
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return null;
        }

        public byte[] ImageToByte(BitmapImage bitmapImage, ImageFormat format)
        {
            BitmapEncoder encoder = null;
            if (format == ImageFormat.Gif) encoder = new GifBitmapEncoder();
            else if (format == ImageFormat.Jpeg) encoder = new JpegBitmapEncoder();
            else if (format == ImageFormat.Png) encoder = new PngBitmapEncoder();
            else encoder = new PngBitmapEncoder();// Default
            try
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return null;
        }

        private void DeleteTemporaryFiles()
        {
            try
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void GetLinksFromServerResponse(string data)
        {
            if (data == null) return;
            if (data.Contains("id=\"warn\""))
            {
                MessageBox.Show(Properties.Resources.MessageServerError, Properties.Resources.Error,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (imageLinks == null) imageLinks = new string[4];
            MatchCollection matches = Regex.Matches(data, "value=\".+\"");
            if (matches.Count != 0)
            {
                imageLinks[0] += matches[0].Value.Replace("value=", "").Replace("\"", "");
                imageLinks[1] += matches[1].Value.Replace("value=", "").Replace("\"", "");
                imageLinks[2] += matches[2].Value.Replace("value=", "").Replace("\"", "");
                imageLinks[3] += matches[3].Value.Replace("value=", "").Replace("\"", "")
                    .Replace("&lt;", "<").Replace("&quot;", "\"").Replace("&gt;", ">");
            }
            imageLinks[1] += '\n';
            imageLinks[2] += '\n';
            if (uploadedImagesCount % Properties.Settings.Default.ImagesPerLine == 0 &&
                uploadedImagesCount > 0)
            {
                imageLinks[0] += "\n\n";
                imageLinks[3] += "\n\n";
            }
            else
            {
                imageLinks[0] += ' ';
                imageLinks[3] += ' ';
            }
        }

        // Progress Bar

        private void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            double value = (ProgressBarUpload.Maximum / ListBoxImages.Items.Count) * (uploadedImagesCount + 1);
            ProgressBarAnimation(value, TimeSpan.FromMilliseconds(250));
        }

        int responsePercent = 5;// Percentage that takes on the upload scale server response

        private void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            double requestPercent = ProgressBarUpload.Maximum - responsePercent;
            double currentUploadPercent = (e.BytesSent * requestPercent / e.TotalBytesToSend);
            double alreadyUploadPercent = 0;
            if (uploadedImagesCount != 0) alreadyUploadPercent = (ProgressBarUpload.Maximum / ListBoxImages.Items.Count) * uploadedImagesCount;
            double value = alreadyUploadPercent + (currentUploadPercent / ListBoxImages.Items.Count);
            ProgressBarAnimation(value, TimeSpan.FromMilliseconds(250));
        }

        private void ProgressBarAnimation(double value, TimeSpan duration)
        {
            if (value < ProgressBarUpload.Minimum || value > ProgressBarUpload.Maximum) return;
            DoubleAnimation doubleAnimation = new DoubleAnimation(value, duration);
            ProgressBarUpload.BeginAnimation(ProgressBar.ValueProperty, doubleAnimation);
        }

        // Other

        private void SetCurrentUICulture()
        {
            if (Properties.Settings.Default.Language == "en" ||
                Properties.Settings.Default.Language == "ru")
            {
                try
                {
                    Thread.CurrentThread.CurrentUICulture =
                        System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                }
                catch (Exception ex)
                {
                    Log.Add(ex.Message);
                }
            }
        }
        
        private void DeleteSelectedImages()
        {
            if (ListBoxImages.SelectedIndex == -1) return;
            while (ListBoxImages.SelectedIndex != -1)
            {
                string filePath = ((Image)ListBoxImages.SelectedItem).Tag.ToString();// Tag contains file path
                ((Image)ListBoxImages.SelectedItem).Source = null;// For unlocking file
                UpdateLayout();
                ListBoxImages.Items.RemoveAt(ListBoxImages.SelectedIndex);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Delete screenshot file
                if (Path.GetDirectoryName(filePath).Split('\\').Last() == Properties.Resources.Screenshots)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Add(ex);
                    }
                }
            }
            UpdateFilesInformation();
        }        

        private void UpdateFilesInformation()
        {
            if (ListBoxImages.Items.Count == 0)
            {
                LabelStatusBar.Content = Properties.Resources.LabelStatusBarReady;
                return;
            }
            long byteSize = 0;
            foreach (Image image in ListBoxImages.Items)
            {
                try
                {
                    byteSize += new FileInfo(new Uri(image.Source.ToString()).LocalPath).Length;
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }
            }
            string information = String.Format("{0}: {1}, {2}: {3}",
                Properties.Resources.LabelStatusBarFiles, ListBoxImages.Items.Count,
                Properties.Resources.LabelStatusBarSize, GetSizeFromBytes(byteSize));
            LabelStatusBar.Content = information;
        }

        private string GetSizeFromBytes(long bytes)
        {
            double size = bytes;
            string name = Properties.Resources.Byte;
            string format = "{0:0} {1}";
            if (size > 1024)
            {
                size /= 1024d;
                name = Properties.Resources.Kilobyte;
                if (size > 1024)
                {
                    size /= 1024d;
                    format = "{0:0.#} {1}";
                    if (size < 1024)
                    {
                        name = Properties.Resources.Megabyte;
                    }
                    else
                    {
                        size /= 1024d;
                        name = Properties.Resources.Gigabyte;
                    }
                }
            }
            return string.Format(format, size, name);
        }

        private void DeleteLinks()
        {
            imageLinks = null;
            TextBoxLinks.Text = String.Empty;
        }

        private void UpdateLinks()
        {
            if (imageLinks == null) return;
            TextBoxLinks.Text = imageLinks[ComboBoxLinksType.SelectedIndex];
        }

        internal static bool IsLogin()
        {
            if (Properties.Settings.Default.UserID != String.Empty &&
                Properties.Settings.Default.PasswordHash != String.Empty)
                return true;
            return false;
        }
    }
}