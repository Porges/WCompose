using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using Application = System.Windows.Application;

namespace WCompose
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon _icon;
        private ComposeKeyboardHook _hook;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            _icon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location),
                Visible = true,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("&Options", (sender, args) => ShowOptions()),
                    new MenuItem("E&xit", (sender, args) => Shutdown()),
                }),
            };

            _icon.Click += (sender, args) => ShowOptions();

            _hook = new ComposeKeyboardHook();

            UpdateTrie(_hook);

            base.OnStartup(e);
        }

        private void ShowOptions()
        {
            MainWindow.Show();
            MainWindow.Activate();
        }

        async void UpdateTrie(ComposeKeyboardHook hook)
        {
            using (var reader = new StreamReader("Compose.pre.txt"))
            {
                hook.SetTrie(await new TrieBuilder().Build(reader));
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
