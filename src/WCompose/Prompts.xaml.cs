using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace WCompose
{
    /// <summary>
    /// Interaction logic for Prompts.xaml
    /// </summary>
    public partial class Prompts 
    {
        public Prompts()
        {
            InitializeComponent();

            SizeChanged += Prompts_SizeChanged;
        }

        void Prompts_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.ActualWidth;
            this.Top = desktopWorkingArea.Bottom - this.ActualHeight;
        }

        private readonly ObservableCollection<string> _items = new ObservableCollection<string>();

        public void SetItems(IEnumerable<string> items)
        {
            _items.Clear();
            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        public ObservableCollection<string> Items
        {
            get { return _items; }
        }
    }
}