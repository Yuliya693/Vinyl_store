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
    /// Логика взаимодействия для AdminMenu.xaml
    /// </summary>
    public partial class AdminMenu : Window
    {
        public AdminMenu()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.Show();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWindow = new OrdersWindow();
            ordersWindow.Show();
        }

        private void OrderHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var orderHistoryWindow = new OrderHistoryWindow();
            orderHistoryWindow.Show();
        }

        private void SalesReportButton_Click(object sender, RoutedEventArgs e)
        {
            var salesReportWindow = new SalesReportWindow();
            salesReportWindow.Show();
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            var salesReportWindow = new AdminRegistrationWindow();
            salesReportWindow.Show();
        }

        private void ManageButton_Click(object sender, RoutedEventArgs e)
        {
            var salesReportWindow = new ManageDataWindow();
            salesReportWindow.Show();
        }

        private void ViewClientButton_Click(object sender, RoutedEventArgs e)
        {
            var salesReportWindow = new ViewClientsWindow();
            salesReportWindow.Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.IsUserLoggedIn = false;
            UserSession.CurrentUserId = -1;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
