using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using VideoLibrary;
using HtmlAgilityPack;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Windows.Threading;
using Syncfusion.UI.Xaml.ProgressBar;
using System.Drawing;
using System.Windows.Navigation;
using System.Diagnostics;

namespace YoutubeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ProgressStatus.Visibility = Visibility.Hidden;
            StatusPercentage.Visibility = Visibility.Hidden;
        }

        private async void ConvertUrl(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(YoutubeUrl.Text))
            {
                try
                {
                    string[] urls = YoutubeUrl.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    ProgressStatus.Visibility = Visibility.Visible;
                    StatusPercentage.Visibility = Visibility.Visible;
                    await Task.Run(() => ExecuteLongProcedureAsync(this, urls));
                    YoutubeUrl.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error message: {ex}");
                }
            }
        }

        internal void ExecuteLongProcedureAsync(MainWindow gui, string[] urls)
        {
            gui.Refresh();
            gui.UpdateWindow("Изтеглянето започна...");
            ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 5, DispatcherPriority.Background);
            foreach (var url in urls)
            {
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 10, DispatcherPriority.Background);
                IWebDriver driver = new ChromeDriver(driverService, options);
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 20, DispatcherPriority.Background);

                var source = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Music\\";
                if (!Directory.Exists(source))
                {
                    Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Music\\");
                }
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 25, DispatcherPriority.Background);

                var youtube = YouTube.Default;
                var vid = youtube.GetVideo(url);
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 30, DispatcherPriority.Background);

                driver.Navigate().GoToUrl(url);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(100);
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 35, DispatcherPriority.Background);

                var videoTitle = driver.FindElement(By.XPath("//div[@id='container']/h1/yt-formatted-string")).Text;
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 40, DispatcherPriority.Background);

                videoTitle = ReplaceInvalidCharacters(videoTitle);
                var fullName = $"{videoTitle}.mp4";
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 45, DispatcherPriority.Background);

                File.WriteAllBytes(source + fullName, vid.GetBytes());
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 50, DispatcherPriority.Background);

                var inputFile = new MediaFile { Filename = source + fullName };
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 55, DispatcherPriority.Background);

                var outputFile = new MediaFile { Filename = $"{source + fullName.Replace("mp4", "mp3")}" };
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 60, DispatcherPriority.Background);

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);
                    ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 70, DispatcherPriority.Background);
                    engine.Convert(inputFile, outputFile);
                    ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 80, DispatcherPriority.Background);
                }

                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 90, DispatcherPriority.Background);
                driver.Close();
                driver.Quit();
                driver.Dispose();
                System.Threading.Thread.Sleep(50);
                ProgressStatus.Dispatcher.Invoke(() => ProgressStatus.Value = 100, DispatcherPriority.Background);
                gui.UpdateWindow($"{Environment.NewLine}{videoTitle} изтеглен успешно.");
            }
            gui.UpdateWindow($"{Environment.NewLine}Изтеглянето приключи.");
            //gui.Refresh();
        }

        void UpdateWindow(string text)
        {
            //safe call
            Dispatcher.Invoke(() =>
            {
                Message.Text += text;
            });
        }

        void Refresh()
        {
            //safe call
            Dispatcher.Invoke(() =>
            {
                Message.Text = string.Empty;
            });
        }

        public string ReplaceInvalidCharacters(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Music";
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start(path);
                }
                else
                {
                    Message.Text = "Не съществува папка 'Music' на работния плот";
                }
            }
            catch (Exception ex)
            {
                Message.Text = ex.Message;
            }
        }
    }
}
