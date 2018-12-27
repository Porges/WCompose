using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WCompose
{
    public partial class Prompts : INotifyPropertyChanged
    {
        public Prompts()
        {
            InitializeComponent();

            SizeChanged += StickToBottomRight;
        }

        void StickToBottomRight(object sender, SizeChangedEventArgs e)
        { 
            const int Margin = 5;

            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - (ActualWidth + Margin);
            Top = desktopWorkingArea.Bottom - (ActualHeight + Margin);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void SetItems(IEnumerable<Tuple<string, string>> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private string _currentInfo;

        public string CurrentInfo
        {
            get { return _currentInfo; }
            set
            {
                _currentInfo = value;
                RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<Tuple<string, string>> Items { get; } =
            new ObservableCollection<Tuple<string, string>>();
    }
}