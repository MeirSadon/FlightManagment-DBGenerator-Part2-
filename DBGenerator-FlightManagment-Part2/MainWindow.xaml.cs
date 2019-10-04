using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using log4net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace DBGenerator_FlightManagment_Part2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel vm = new ViewModel();
        public static Dispatcher m_Dispatcher;
        public MainWindow()
        {
            InitializeComponent();
            m_Dispatcher = Application.Current.Dispatcher;
            this.DataContext = vm;
            LoggingLstBx.ItemsSource = ViewModel.Logger;
            ((INotifyCollectionChanged)LoggingLstBx.Items).CollectionChanged += ListView_CollectionChanged;
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                LoggingLstBx.ScrollIntoView(e.NewItems[0]);
            }
        }
    }
}
