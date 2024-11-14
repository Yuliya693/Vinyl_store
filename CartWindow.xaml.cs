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
    /// <summary>
    /// Логика взаимодействия для CartWindow.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        public static List<CatalogProduct> CartItems { get; private set; } = new List<CatalogProduct>();

        public CartWindow()
        {
            InitializeComponent();
            LoadCartItems();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadCartItems()
        {
            CartItemsListView.ItemsSource = null;
            CartItemsListView.ItemsSource = CatalogWindow.CartItems;
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            TotalAmountTextBlock.Text = $"Общая сумма: {CatalogWindow.CartItems.Sum(item => item.ProductPrice * item.Quantity)} р.";
        }

        private void DecreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CatalogProduct product)
            {
                if (product.Quantity > 1)
                {
                    product.Quantity--;
                    LoadCartItems();
                }
            }
        }

        private void IncreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CatalogProduct product)
            {
                if (product.Quantity < product.AvailableQuantity)
                {
                    product.Quantity++;
                    LoadCartItems();
                }
                else
                {
                    MessageBox.Show("Невозможно добавить больше товара в корзину, чем имеется в наличии.");
                }
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CatalogProduct product)
            {
                CatalogWindow.CartItems.Remove(product);
                LoadCartItems();
            }
        }

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsUserLoggedIn && UserSession.CurrentUserId > 0)
            {
                try
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        // Создание уникального кода заказа
                        string orderCode = GenerateOrderCode();

                        // Вставка нового заказа
                        string insertOrderQuery = "INSERT INTO Orders (OrderUser, OrderDateTime, OrderStatus, OrderCode) VALUES (@OrderUser, @OrderDateTime, @OrderStatus, @OrderCode)";
                        using (var command = new SQLiteCommand(insertOrderQuery, connection))
                        {
                            command.Parameters.AddWithValue("@OrderUser", UserSession.CurrentUserId);
                            command.Parameters.AddWithValue("@OrderDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@OrderStatus", 1); // Assuming 1 is the default status ID for "Оформлен"
                            command.Parameters.AddWithValue("@OrderCode", orderCode);
                            command.ExecuteNonQuery();
                        }

                        // Получение ID созданного заказа
                        string getOrderIdQuery = "SELECT last_insert_rowid()";
                        int orderId;
                        using (var command = new SQLiteCommand(getOrderIdQuery, connection))
                        {
                            orderId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Вставка элементов заказа
                        foreach (var item in CatalogWindow.CartItems)
                        {
                            string insertOrderItemQuery = "INSERT INTO OrderItems (Order_ID, Product_ID, OrderItemCount, OrderItemPrice) VALUES (@Order_ID, @Product_ID, @OrderItemCount, @OrderItemPrice)";
                            using (var command = new SQLiteCommand(insertOrderItemQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Order_ID", orderId);
                                command.Parameters.AddWithValue("@Product_ID", item.ProductID);
                                command.Parameters.AddWithValue("@OrderItemCount", item.Quantity);
                                command.Parameters.AddWithValue("@OrderItemPrice", item.ProductPrice);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Уменьшение количества товара на складе
                        foreach (var item in CatalogWindow.CartItems)
                        {
                            string updateProductCountQuery = "UPDATE Product SET ProductCount = ProductCount - @OrderItemCount WHERE ProductID = @OrderItemProduct";
                            using (var command = new SQLiteCommand(updateProductCountQuery, connection))
                            {
                                command.Parameters.AddWithValue("@OrderItemCount", item.Quantity);
                                command.Parameters.AddWithValue("@OrderItemProduct", item.ProductID);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Очистка корзины после оформления заказа
                        CatalogWindow.CartItems.Clear();
                        LoadCartItems();

                        // Открытие окна с кодом заказа
                        OrderCodeWindow orderCodeWindow = new OrderCodeWindow(orderCode);
                        orderCodeWindow.Show();
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в свой профиль, чтобы оформить заказ.");
            }
        }



        private string GenerateOrderCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }


}
