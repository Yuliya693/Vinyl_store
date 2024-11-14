using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
    public partial class OrdersWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadOrders()
        {
            List<CustomerOrder> orders = new List<CustomerOrder>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT o.OrderID, o.OrderDateTime, u.UserName, os.OrderStatusName, o.OrderCode
                                 FROM Orders o
                                 JOIN Users u ON o.OrderUser = u.UserID
                                 JOIN OrderStatus os ON o.OrderStatus = os.OrderStatusID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new CustomerOrder
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderDateTime = Convert.ToDateTime(reader["OrderDateTime"]),
                                UserName = reader["UserName"].ToString(),
                                OrderStatus = reader["OrderStatusName"].ToString(),
                                OrderCode = reader["OrderCode"].ToString()
                            };
                            orders.Add(order);
                        }
                    }
                }
            }

            OrdersListView.ItemsSource = orders;
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListView.SelectedItem is CustomerOrder selectedOrder)
            {
                var orderDetailsWindow = new OrderDetailsWindow(selectedOrder.OrderID);
                orderDetailsWindow.ShowDialog();
                LoadOrders();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            // Закрываем текущее окно
            this.Close();
        }
    }

    public class CustomerOrder
    {
        public int OrderID { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string UserName { get; set; }
        public string OrderStatus { get; set; }
        public string OrderCode { get; set; }
    }
}

