using System.Drawing;
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
        
        protected override void OnStartup(StartupEventArgs e)
        {
            _icon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location),
                Visible = true,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("E&xit", (sender, args) => Shutdown()),
                }),
            };

            _icon.Click += (sender, args) =>
            {
                MainWindow.Show();
                MainWindow.Activate();
            };

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();

            base.OnExit(e);
        }
    }
}
