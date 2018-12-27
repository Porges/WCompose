using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace WCompose
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "Disposal happens in OnExit method")]
    public partial class App : Application
    {
        private NotifyIcon _icon;
        private ComposeKeyboardHook _hook;
        private MenuItem _startOnStartupMenuItem;

        protected override void OnStartup(StartupEventArgs e)
        {
            var assemblyVersion = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;

            _startOnStartupMenuItem =
                new MenuItem("Start on startup", (sender, args) => ToggleStartOnStartup())
                {
                    Checked = GetStartOnStartup()
                };

            _icon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location),
                Visible = true,
                Text = $"WCompose {assemblyVersion}",
                ContextMenu = new ContextMenu(new[]
                {
                    _startOnStartupMenuItem,            
                    new MenuItem("-"),
                    new MenuItem("E&xit", (sender, args) => Shutdown()),
                }),
            };
            
            _hook = new ComposeKeyboardHook();

            // load in background, not on UI thread
            Task.Run(() => LoadConfigFile(_hook));

            base.OnStartup(e);
        }

        private static async Task LoadSettingsFile(TextReader file, ComposeKeyboardHook hook)
        {
            hook.Trie = await new JsonTrieBuilder().Build(file);
        }

        private static string GetUserSettingsPath()
        {
            const string UserFile = "Mappings.json";

            var userDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(userDataDirectory, "WCompose", UserFile);
        }

        private static string GetDefaultSettingsPath()
        {
            const string DefaultFile = "DefaultMappings.json";

            var appDirectory = Environment.CurrentDirectory;

            return Path.Combine(appDirectory, DefaultFile);
        }

        private static TextReader OpenSettingsFile()
        {
            try
            {
                return new StreamReader(GetUserSettingsPath());
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return new StreamReader(GetDefaultSettingsPath());
            }
        }

        private static async Task LoadConfigFile(ComposeKeyboardHook hook)
        {
            try
            {
                using (var fs = OpenSettingsFile())
                {
                    await LoadSettingsFile(fs, hook);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error loading settings file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "WCompose";

        private RegistryKey OpenRunKey(bool writable) => Registry.CurrentUser.OpenSubKey(RunKey, writable);

        private static readonly string StartupPath =
            "\"" +
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs",
                    "WCompose",
                    "WCompose.appref-ms")
            + "\"";

        private bool GetStartOnStartup()
        {
            using (var key = OpenRunKey(true))
            {
                var path = key.GetValue(ValueName);
                if (path == null)
                {
                    return false;
                }
                
                // check if path needs updating
                if (path as string != StartupPath)
                {
                    key.SetValue(ValueName, StartupPath);
                }

                return true;
            }
        }

        private void ToggleStartOnStartup()
        {
            using (var key = OpenRunKey(true))
            {
                var enabled = _startOnStartupMenuItem.Checked;
                if (enabled)
                {
                    key.DeleteValue(ValueName);
                }
                else
                {
                    key.SetValue(ValueName, StartupPath);
                }

                _startOnStartupMenuItem.Checked = !enabled;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();

            _hook.Dispose();

            base.OnExit(e);
        }
    }
}
