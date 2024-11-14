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
    public partial class OrderDetailsWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        private int orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            LoadOrderDetails();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadOrderDetails()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT o.OrderCode, o.OrderDateTime, u.UserName, os.OrderStatusName
                                 FROM Orders o
                                 JOIN Users u ON o.OrderUser = u.UserID
                                 JOIN OrderStatus os ON o.OrderStatus = os.OrderStatusID
                                 WHERE o.OrderID = @OrderId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            OrderCodeTextBlock.Text = $"Код заказа: {reader["OrderCode"].ToString()}";
                            UserNameTextBlock.Text = $"Имя клиента: {reader["UserName"].ToString()}";
                            OrderDateTimeTextBlock.Text = $"Дата и время заказа: {reader["OrderDateTime"].ToString()}";
                        }
                    }
                }

                List<OrderItem> orderItems = new List<OrderItem>();
                query = @"SELECT p.ProductName, oi.OrderItemCount, oi.OrderItemPrice
                          FROM OrderItems oi
                          JOIN Product p ON oi.Product_ID = p.ProductID
                          WHERE oi.Order_ID = @OrderId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var orderItem = new OrderItem
                            {
                                ProductName = reader["ProductName"].ToString(),
                                Quantity = Convert.ToInt32(reader["OrderItemCount"]),
                                ProductPrice = Convert.ToDecimal(reader["OrderItemPrice"]),
                                TotalPrice = Convert.ToInt32(reader["OrderItemCount"]) * Convert.ToDecimal(reader["OrderItemPrice"])
                            };
                            orderItems.Add(orderItem);
                        }
                    }
                }
                OrderItemsListView.ItemsSource = orderItems;
                TotalAmountTextBlock.Text = $"Общая сумма: {orderItems.Sum(item => item.TotalPrice)} р.";
            }
        }

        private void IssueOrderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string updateOrderStatusQuery = "UPDATE Orders SET OrderStatus = 2 WHERE OrderID = @OrderId"; // Assuming 2 is the status ID for "Выдан"
                using (var command = new SQLiteCommand(updateOrderStatusQuery, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    command.ExecuteNonQuery();
                }
            }
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

