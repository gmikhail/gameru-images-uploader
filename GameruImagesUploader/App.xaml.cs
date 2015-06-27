using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;

namespace GameruImagesUploader
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;
        internal static Mutex mutex;
        private IPAddress address = IPAddress.Loopback;
        private int port = 51736;
        private TcpListener server = null;
        private TcpClient client = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                bool isNewInstance = false;
                mutex = new Mutex(true, Assembly.GetEntryAssembly().GetName().Name, out isNewInstance);

                if (isNewInstance)
                {
                    PrimaryInstance();
                    ArgumentsProcessing(e.Args.ToList());
                }
                else
                {
                    SecondaryInstance(e);
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void PrimaryInstance()
        {
            mainWindow = new MainWindow();
            InitializeNotifyIcon();

            try
            {
                Thread thread = new Thread(() =>
                {
                    server = new TcpListener(address, port);
                    server.Start();

                    Byte[] bytes = new Byte[Byte.MaxValue + 1];
                    String data = null;

                    while (true)
                    {
                        TcpClient client = server.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();

                        int readBytes;
                        while ((readBytes = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.Default.GetString(bytes, 0, readBytes);
                        }
                        client.Close();

                        List<string> argsList = null;
                        if (data != null)
                        {
                            argsList = new List<string>(data.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                        }

                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            // Window appears if run a second copy of the application (without parameters)
                            if (data == null) mainWindow.Show();
                            mainWindow.Activate();

                            if(argsList != null) ArgumentsProcessing(argsList);
                        }));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void SecondaryInstance(StartupEventArgs e)
        {
            NetworkStream stream = null;
            try
            {
                client = new TcpClient(address.ToString(), port);
                string msg = String.Empty;
                foreach (string arg in e.Args)
                    msg += arg + '\n';
                Byte[] data = System.Text.Encoding.Default.GetBytes(msg);
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
            finally
            {
                if(stream != null) stream.Close();
                if(client != null) client.Close();
                Application.Current.Shutdown();
            }
        }

        private void ArgumentsProcessing(List<string> argsList)
        {
            if (argsList == null) return;

            try
            {
                bool autoUpload = false;
                if (argsList.Contains(GameruImagesUploader.Properties.Resources.LaunchKeyUpload))
                {
                    autoUpload = true;
                    argsList.RemoveAll(s => s.Equals(GameruImagesUploader.Properties.Resources.LaunchKeyUpload));
                }
                if (argsList.Contains(GameruImagesUploader.Properties.Resources.LaunchKeyHide))
                {
                    argsList.RemoveAll(s => s.Equals(GameruImagesUploader.Properties.Resources.LaunchKeyHide));
                    if (GameruImagesUploader.Properties.Settings.Default.NotificationAreaClose == true ||
                        GameruImagesUploader.Properties.Settings.Default.NotificationAreaMinimize == true)
                    {
                        mainWindow.Hide();
                    }
                    else mainWindow.WindowState = WindowState.Minimized;
                }
                else
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                }

                mainWindow.AddImages(argsList.ToArray());

                if (autoUpload == true && mainWindow.isUploading == false)
                {
                    mainWindow.UploadAllImages();
                }
            }
            catch(Exception ex)
            {
                Log.Add(ex);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mainWindow != null)
            {
                notifyIcon.Dispose();
                mainWindow.keyboardListener.UnHookKeyboard();
            }
        }

        // Notify Icon

        internal static System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

        private void InitializeNotifyIcon()
        {
            notifyIcon.Icon = new System.Drawing.Icon(GameruImagesUploader.Properties.Resources.IconLogo, new System.Drawing.Size(16, 16));
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add(GameruImagesUploader.Properties.Resources.Show, NotifyIconContextMenu_Click);
            var menuItemMakeScreenshots = new System.Windows.Forms.MenuItem(GameruImagesUploader.Properties.Resources.MakeScreenshots, NotifyIconContextMenu_Click);
            menuItemMakeScreenshots.Checked = GameruImagesUploader.Properties.Settings.Default.MakeScreenshots;
            notifyIcon.ContextMenu.MenuItems.Add(menuItemMakeScreenshots);
            notifyIcon.ContextMenu.MenuItems.Add(GameruImagesUploader.Properties.Resources.Settings, NotifyIconContextMenu_Click);
            notifyIcon.ContextMenu.MenuItems.Add("-");
            notifyIcon.ContextMenu.MenuItems.Add(GameruImagesUploader.Properties.Resources.Exit, NotifyIconContextMenu_Click);
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void NotifyIconContextMenu_Click(object sender, EventArgs e)
        {
            string menuItem = ((System.Windows.Forms.MenuItem)sender).Text;
            if (menuItem == GameruImagesUploader.Properties.Resources.Show)
            {
                mainWindow.Show();
            }
            else if (menuItem == GameruImagesUploader.Properties.Resources.MakeScreenshots)
            {
                var newValue = !((System.Windows.Forms.MenuItem)sender).Checked;
                notifyIcon.ContextMenu.MenuItems[1].Checked = newValue;
                GameruImagesUploader.Properties.Settings.Default.MakeScreenshots = newValue;
                GameruImagesUploader.Properties.Settings.Default.Save();
            }
            else if (menuItem == GameruImagesUploader.Properties.Resources.Settings)
            {
                var settingsWindow = new SettingsWindow();
                if (mainWindow.Visibility == System.Windows.Visibility.Visible) settingsWindow.Owner = mainWindow;
                else settingsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                settingsWindow.ShowDialog();
            }
            else if (menuItem == GameruImagesUploader.Properties.Resources.Exit)
            {
                Application.Current.Shutdown();
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            mainWindow.Show();
        }
    }
}