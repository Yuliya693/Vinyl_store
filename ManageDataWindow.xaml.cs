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
    /// Логика взаимодействия для ManageDataWindow.xaml
    /// </summary>
    public partial class ManageDataWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public ManageDataWindow()
        {
            InitializeComponent();
            LoadProducts();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadProducts(string searchQuery = "")
        {
            List<Product> products = new List<Product>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT p.ProductID, p.ProductName, a.ArtistName AS ProductArtistName, p.ProductCount, p.ProductPrice, g.GenreName AS ProductGenre, 
                                 p.ProductYear, p.ProductDiscription AS ProductDescription, p.ProductPhoto, ps.ProductStatusName AS ProductStatus, 
                                 s.StyleName AS ProductStyle, c.CountryName AS ProductCountry 
                                 FROM Product p
                                 JOIN Artist a ON p.ProductArtist = a.ArtistID
                                 JOIN Genre g ON p.ProductGenre = g.GenreID
                                 JOIN ProductStatus ps ON p.ProductStatus = ps.ProductStatusID
                                 JOIN Style s ON p.ProductStyle = s.StyleID
                                 JOIN Country c ON p.ProductCountry = c.CountryID
                                 WHERE p.ProductName LIKE @searchQuery OR a.ArtistName LIKE @searchQuery";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ProductID = int.Parse(reader["ProductID"].ToString()),
                                ProductName = reader["ProductName"].ToString(),
                                ProductArtistName = reader["ProductArtistName"].ToString(),
                                ProductCount = int.Parse(reader["ProductCount"].ToString()),
                                ProductPrice = decimal.Parse(reader["ProductPrice"].ToString()),
                                ProductGenre = reader["ProductGenre"].ToString(),
                                ProductYear = int.Parse(reader["ProductYear"].ToString()),
                                ProductDescription = reader["ProductDescription"].ToString(),
                                ProductPhoto = reader["ProductPhoto"] != DBNull.Value ? LoadImage((byte[])reader["ProductPhoto"]) : null,
                                ProductStatus = reader["ProductStatus"].ToString(),
                                ProductStyle = reader["ProductStyle"].ToString(),
                                ProductCountry = reader["ProductCountry"].ToString()
                            };
                            products.Add(product);
                        }
                    }
                }
            }

            ProductsDataGrid.ItemsSource = products;
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var stream = new System.IO.MemoryStream(imageData))
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text);
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            if (addProductWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                EditProductWindow editProductWindow = new EditProductWindow(selectedProduct);
                if (editProductWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.");
            }
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Product WHERE ProductID = @productID";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@productID", selectedProduct.ProductID);
                            command.ExecuteNonQuery();
                        }
                    }
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            
            // Закрываем текущее окно
            this.Close();
        }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public BitmapImage ProductPhoto { get; set; }
        public string ProductName { get; set; }
        public string ProductArtistName { get; set; }
        public int ProductCount { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductGenre { get; set; }
        public int ProductYear { get; set; }
        public string ProductDescription { get; set; }
        public string ProductStatus { get; set; }
        public string ProductStyle { get; set; }
        public string ProductCountry { get; set; }
    }
}
