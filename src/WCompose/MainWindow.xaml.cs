using System;
using System.ComponentModel;
using System.Windows;

namespace WCompose
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ComposeKeyboardHook _hook;

        public MainWindow()
        {
            InitializeComponent();

            _hook = new ComposeKeyboardHook();

            Closing += (sender, args) =>
            {
                args.Cancel = true;
                Hide();
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _hook.Dispose();
            base.OnClosing(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }
    }
}
