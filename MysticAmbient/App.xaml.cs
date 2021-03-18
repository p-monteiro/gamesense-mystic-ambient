using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using MysticAmbient.Utils;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using MysticAmbient.ViewModels;

namespace MysticAmbient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        private TaskbarIcon notifyIcon;
        private Timer refreshIconTimer;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Debug.WriteLine("Startup");

            //create the notifyicon (it's a resource declared in Resources/NotifyIcon.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            DrawIcon();

            // I HATE BLURRY ICONS
            refreshIconTimer = new Timer(1000);
            //refreshIconTimer.Elapsed += (sender, e) => DrawIcon();
            refreshIconTimer.Start();


        }


        protected override void OnExit(ExitEventArgs e)
        {
            refreshIconTimer.Stop();
            refreshIconTimer.Dispose();

            notifyIcon.Visibility = Visibility.Hidden;
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner 

            base.OnExit(e);
        }

        private void DrawIcon()
        {
            var uri_s = string.Format(
                "pack://application:,,,/{0};component/{1}"
                , Assembly.GetExecutingAssembly().GetName().Name
                , WindowsThemeManager.GetWindowsTheme() == WindowsThemeManager.WindowsTheme.Light ? "Assets/Icons/logo-black-outline.ico" : "Assets/Icons/logo-white-full.ico"
            );
            var uri = new Uri(uri_s);
            ImageSource imageSource = new BitmapImage(uri);

            notifyIcon.IconSource = imageSource;
        }
    }
}
