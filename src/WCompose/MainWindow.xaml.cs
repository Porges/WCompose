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

        private Task SaveXComposeFile(string file)
        {
            throw new NotImplementedException();
        }

        private async Task LoadSettingsFile(string file)
        {
            _hook.Trie = await new JsonTrieBuilder().Build(file);
        }

        private string GetUserSettingsPath()
        {
            const string UserFile = "Mappings.json";

            var userDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(userDataDirectory, "WCompose", UserFile);
        }

        private static String GetDefaultSettingsPath()
        {
            const string DefaultFile = "DefaultMappings.json";

            var appDirectory = Environment.CurrentDirectory;

            return Path.Combine(appDirectory, DefaultFile);
        }

        private string GetSettingsFile()
        {
            var userPath = GetUserSettingsPath();
            if (File.Exists(userPath))
            {
                return userPath;
            }

            return GetDefaultSettingsPath();
        }

        private Task SaveConfig()
        {
            throw new NotImplementedException();
        }

        private async void LoadConfig()
        {
            try
            {
                await LoadSettingsFile(GetSettingsFile());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading settings file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    await LoadSettingsFile(dialog.FileName);
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
