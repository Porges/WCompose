using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
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

            MainWindow = new MainWindow(_hook);

            base.OnStartup(e);
        }

        private void ShowOptions()
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.BringIntoView();
            MainWindow.Activate();
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
