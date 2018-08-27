using Microsoft.Win32;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WallpaperLens.Events;
using WallpaperLens.Utilities;

namespace WallpaperLens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEventAggregator _eventAggregator;
        private static Bot _bot;

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uiAction, int uiParam,
        string pvParam, uint fWinIni);

        public MainWindow()
        {
            InitializeComponent();

            _eventAggregator = new EventAggregator();
            _bot = new Bot(_eventAggregator);

            _eventAggregator.GetEvent<NewTrendEvent>().Subscribe(SetImage, ThreadOption.UIThread);

            MouseDown += MainWindow_MouseDown;

            if (File.Exists(@"D:\bg.png"))
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;

                File.Copy(@"D:\bg.png", path + "bg.png", true);

                Img.Source = new BitmapImage(new Uri(path + "bg.png"));
            }
            else
            {
                Img.Source = new BitmapImage(new Uri("pack://application:,,,/WallpaperLens;component/Resources/test.jpg"));
            }

            _bot.Run();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void SetImage(string title)
        {
            lblTitle.Content = title;

            var path = AppDomain.CurrentDomain.BaseDirectory;
            Img.ChangeSource(new BitmapImage(new Uri(path + "bg.png")), new TimeSpan(0, 0, 0, 0, 500), new TimeSpan(0, 0, 0, 0, 500));
        }

        private void btnMinimise_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void btnSetWall_Click(object sender, RoutedEventArgs e)
        {
            SetWall();
        }

        private void SetWall()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue("WallpaperStyle", "2");
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, @"D:\bg.png", SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                SetWall();
            }
        }
    }
}
