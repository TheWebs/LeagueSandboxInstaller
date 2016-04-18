using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using CG.Web.MegaApiClient;
using System.IO;
using SharpCompress;
using SharpCompress.Archive;
using SharpCompress.Reader;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace LeagueSandboxInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        string appData;
        public MainWindow()
        {
            InitializeComponent();
            appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public void UpdateStatus(double value)
        {
            pr.Value = Convert.ToInt32(value);
            label.Content = Convert.ToInt32(value).ToString() + "%";
            Console.WriteLine(value);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            //warning
            MessageBox.Show("Next you'll be asked to choose a folder. Keep in mind that if you choose \"C:\\LOL_4.20\" the result will be \"C:\\LOL_4.20\\League of Legends\"");
            //Select folder
            path = ShowFolderBrowserDialog();
            //Start the download
            label1.Content = "Stats: Downloading ...";
            var client = new MegaApiClient();
            client.LoginAnonymous();
            Progress<double> ze = new System.Progress<double>(p => UpdateStatus(p));
            if (File.Exists(appData + "\\ze.rar")) { File.Delete(appData + "\\ze.rar"); }  
            await client.DownloadFileAsync(new Uri("https://mega.nz/#!pFRVxBJQ!AMbsJnS9kqhvQ-tfP8QxoBikbrjlGQ4MdzNYGo0fIKM"), appData + "\\ze.rar", ze); //smaller test file https://mega.nz/#!J90AhTAI!Piq-76v6tB6l6W2HexqoN9XU8qvGdBJ6CONFMEyCPqE
            StartExtracting();

        }


        private void StartExtracting()
        {
            label.Content = "0%";
            label1.Content = "Extracting ...";
            IArchive rar = SharpCompress.Archive.Rar.RarArchive.Open(new FileInfo(appData + "\\ze.rar"), SharpCompress.Common.Options.None);

            long position = 0;
            long size = rar.TotalSize;
            float progress = 0;
            //create folders
            foreach (SharpCompress.Common.Rar.RarEntry entry in rar.Entries)
            {
                if(entry.IsDirectory)
                Directory.CreateDirectory(System.IO.Path.Combine(path, entry.Key));
            }
            foreach (var entry in rar.Entries)
            {
                try
                {
                    entry.WriteToFile(System.IO.Path.Combine(path, entry.Key), SharpCompress.Common.ExtractOptions.Overwrite);
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    //shithole
                    Console.WriteLine("Shitty access exception -.-");
                }
                position += entry.CompressedSize;
                progress = (float)position / size * 100;
                pr.Value = progress;
                Console.WriteLine("Progress: {0}%", progress);
                label.Content = ((int)progress).ToString() + "%";
                DoEvents();
                if ((int)(progress) == 100)
                    break;
                
            }

            label1.Content = "Stats: Finished extracting!";
            button.Visibility = Visibility.Hidden;
            button2.Visibility = Visibility.Visible;
            rar.Dispose();

        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void label2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private string ShowFolderBrowserDialog()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select 4.20 folder";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            dialog.ShowDialog();
            return dialog.SelectedPath;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            File.Delete(appData + "\\ze.rar");
            Environment.Exit(0);
        }
    }
}
