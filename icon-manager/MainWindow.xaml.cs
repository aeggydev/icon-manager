using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;

namespace icon_manager
{
    /// <summary>
    ///     Interaction logic for MainWinSelectPng/summary>
    public partial class MainWindow : Window
    {
        public List<Directory> Directories { get; set; } = new List<Directory>();
        public MainWindow()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var lol = Fonts.GetFontFamilies(new Uri("pack://application:,,,/Fonts/#"));
            InitializeComponent();
            PathList.ItemsSource = Directories;
        }

        private void OnSelectFolder(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var rootPath = dlg.SelectedPath;
                var rootDir = Directory.FromPath(rootPath);
                var dirs = rootDir.RecurseToList();

                Directories = Directories.Concat(dirs).ToList();
                PathList.ItemsSource = Directories;
                PathList.Items.Refresh();
            }
        }

        private void RemovePath(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            Directories.Remove(btn.DataContext as Directory);
            PathList.ItemsSource = Directories;
            PathList.Items.Refresh();
        }
    }
}