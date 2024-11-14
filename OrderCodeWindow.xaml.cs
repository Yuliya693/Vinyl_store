using System;
using System.Collections.Generic;
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

namespace Vinyl_store
{
    /// <summary>
    /// Логика взаимодействия для OrderCodeWindow.xaml
    /// </summary>
    public partial class OrderCodeWindow : Window
    {
        public OrderCodeWindow(string orderCode)
        {
            InitializeComponent();
            OrderCodeTextBlock.Text = orderCode;
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void BackToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }
    }
}
