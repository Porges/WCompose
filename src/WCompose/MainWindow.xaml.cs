using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace WCompose
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ComposeKeyboardHook _hook;
        
        public MainWindow(ComposeKeyboardHook hook)
        {
            _hook = hook;

            InitializeComponent();

            Closing += (sender, args) =>
            {
                args.Cancel = true;
                Hide();
            };

            LoadConfig();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private async Task SaveXComposeFile(string file)
        {
            using (var writer = new StreamWriter(file))
            {
                await new TrieWriter().Write(writer, _hook.Trie);
            }
        }

        private async Task LoadXComposeFile(string file)
        {
            using (var reader = new StreamReader(file))
            {
                _hook.Trie = await new TrieBuilder().Build(reader);
            }
        }

        private string GetDefaultConfigFile()
        {
            const string ConfigName = "WCompose.keys";

            var directory = ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.DataDirectory
                : Environment.CurrentDirectory;
            
            return Path.Combine(directory, ConfigName);;
        }

        private async Task SaveConfig()
        {
            using (var writer = new StreamWriter(GetDefaultConfigFile()))
            {
                await new TrieWriter().Write(writer, _hook.Trie);
            }
        }

        private async void LoadConfig()
        {
            try
            {
                await LoadXComposeFile(GetDefaultConfigFile());
            }
            catch (FileNotFoundException)
            { }
        }

        private async void LoadXComposeFileClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Title = "Please select an XCompose file",
                };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await LoadXComposeFile(dialog.FileName);
                    await SaveConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error opening XCompose file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveXComposeFileClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    DefaultExt = ".keys"
                };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await SaveXComposeFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error saving XCompose file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
