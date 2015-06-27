using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void TextBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateLoginButtonState();
        }

        private void UpdateLoginButtonState()
        {
            int usernameMinLength = 3;
            int passwordMinLength = 3;
            if (TextBoxUsername.Text.Length >= usernameMinLength && 
                PasswordBox.SecurePassword.Length >= passwordMinLength)
                ButtonOK.IsEnabled = true;
            else ButtonOK.IsEnabled = false;
        } 
        
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.OriginalString);
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            if (Login(TextBoxUsername.Text, PasswordBox.SecurePassword) == true)
            {
                DialogResult = true;
            }
            else
            {
                ((Button)sender).IsEnabled = true;
                PasswordBox.Clear();
            }
        }

        private bool Login(string username, SecureString password)
        {
            if (username.Length == 0 || password.Length == 0) return false;

            try
            {
                // Preparing request
                string data = string.Format("UserName={0}&PassWord={1}&CookieDate=1", username, SecureStringToString(password));
                string method = "POST";
                string contentType = "application/x-www-form-urlencoded";
                NameValueCollection valueCollection = new NameValueCollection();
                valueCollection.Add("Cookie", Properties.Resources.CookieNameSessionId + '=' + GenerateSessionId());

                // Sending request and receive response
                var response = SendRequest(Properties.Resources.LoginUrl, data, method, contentType, valueCollection);

                // Analysis of response
                if (response != null)
                {
                    if (response.Cookies[Properties.Resources.CookieNameUserId] != null &&
                        response.Cookies[Properties.Resources.CookieNamePasswordHash] != null)
                    {
                        string userId = response.Cookies[Properties.Resources.CookieNameUserId].Value;
                        string passwordHash = response.Cookies[Properties.Resources.CookieNamePasswordHash].Value;
                        Properties.Settings.Default.UserID = userId;
                        Properties.Settings.Default.PasswordHash = passwordHash;
                        Properties.Settings.Default.UserName = username;
                        Properties.Settings.Default.Save();
                        GetUserAvatar(userId);
                        return true;
                    }
                    else MessageBox.Show(Properties.Resources.LoginError, Properties.Resources.Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
            return false;
        }

        private HttpWebResponse SendRequest(string uri, string data, string method, 
            string contentType, NameValueCollection valueCollection)
        {
            try
            {
                var request = WebRequest.Create(uri) as HttpWebRequest;
                request.ContentType = contentType;
                request.Method = method;
                request.Headers.Add(valueCollection);
                byte[] bytes = Encoding.Default.GetBytes(data);
                request.ContentLength = bytes.Length;
                using (Stream os = request.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                request.CookieContainer = new CookieContainer();
                return request.GetResponse() as HttpWebResponse;
            }
            catch (Exception ex) 
            {
                Log.Add(ex);
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error); 
            }
            return null;
        }

        private void GetUserAvatar(string userId)
        {
            // Delete old avatar
            string filePath = Properties.Settings.Default.AvatarPath;
            if(filePath != string.Empty)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch { }
            }

            // Search and save new avatar
            string rawUrl = "http://www.gameru.net/forum/uploads/av-" + userId;
            string[] fileExtensions = { "gif", "jpg", "jpeg", "png" };
            string rawFilePath = String.Format("{0}\\{1}\\Avatar", 
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);
            foreach (string extension in fileExtensions)
            {
                string url = rawUrl + '.' + extension;
                using (var wc = new WebClient())
                {
                    try
                    {
                        filePath = rawFilePath + '.' + extension;
                        wc.DownloadFile(url, filePath);
                        Properties.Settings.Default.AvatarPath = filePath;
                        Properties.Settings.Default.Save();
                        return;
                    }
                    catch { }
                }
            }
        }

        private string GenerateSessionId(int length = 32)
        {
            var random = new Random();
            string result = string.Empty;
            while (result.Length < length)
            {
                result += random.Next(16).ToString("X");
            }
            return result.ToLower();
        }

        private static string SecureStringToString(SecureString value)
        {
            IntPtr bstr = new IntPtr();
            try
            {
                bstr = Marshal.SecureStringToBSTR(value);
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}