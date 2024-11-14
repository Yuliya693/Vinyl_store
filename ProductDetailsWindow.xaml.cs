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
    /// Логика взаимодействия для ProductDetailsWindow.xaml
    /// </summary>
    public partial class ProductDetailsWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        private int productId;
        private CatalogProduct product;
        public static List<CatalogProduct> CartItems { get; private set; } = new List<CatalogProduct>();

        public ProductDetailsWindow(int productId)
        {
            InitializeComponent();
            this.productId = productId;
            LoadProductDetails();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadProductDetails()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT p.ProductID, p.ProductName, a.ArtistName AS ProductArtistName, p.ProductPrice, p.ProductPhoto, g.GenreName AS ProductGenre, 
                             ps.ProductStatusName AS ProductStatus, c.CountryName AS ProductCountry, s.StyleName AS ProductStyle, p.ProductDiscription AS ProductDescription, p.ProductCount AS ProductQuantity
                             FROM Product p
                             JOIN Artist a ON p.ProductArtist = a.ArtistID
                             JOIN Genre g ON p.ProductGenre = g.GenreID
                             JOIN ProductStatus ps ON p.ProductStatus = ps.ProductStatusID
                             JOIN Country c ON p.ProductCountry = c.CountryID
                             JOIN Style s ON p.ProductStyle = s.StyleID
                             WHERE p.ProductID = @productId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@productId", productId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new CatalogProduct
                            {
                                ProductID = int.Parse(reader["ProductID"].ToString()),
                                ProductName = reader["ProductName"].ToString(),
                                ProductArtistName = reader["ProductArtistName"].ToString(),
                                ProductPrice = decimal.Parse(reader["ProductPrice"].ToString()),
                                ProductPhoto = reader["ProductPhoto"] != DBNull.Value ? LoadImage((byte[])reader["ProductPhoto"]) : null,
                                ProductGenre = reader["ProductGenre"].ToString(),
                                ProductStatus = reader["ProductStatus"].ToString(),
                                ProductCountry = reader["ProductCountry"].ToString(),
                                ProductStyle = reader["ProductStyle"].ToString(),
                                ProductDiscription = reader["ProductDescription"].ToString(),
                                AvailableQuantity = int.Parse(reader["ProductQuantity"].ToString())
                            };

                            ProductTitle.Text = product.ProductName;
                            ProductArtist.Text = product.ProductArtistName;
                            ProductPrice.Text = $"{product.ProductPrice} р.";
                            ProductDetails.Text = $"Жанр: {product.ProductGenre}\nСтатус: {product.ProductStatus}\nСтрана: {product.ProductCountry}\nСтиль: {product.ProductStyle}\nОписание: {product.ProductDiscription}";
                            ProductQuantity.Text = $"Количество: {product.AvailableQuantity}";

                            if (product.ProductPhoto != null)
                            {
                                ProductImage.Source = product.ProductPhoto;
                            }
                        }
                    }
                }
            }
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

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsUserLoggedIn)
            {
                MessageBox.Show("Войдите в свой профиль, чтобы добавить товар в корзину.");
                return;
            }

            // Убедитесь, что количество устанавливается перед добавлением в корзину
            product.Quantity = 1; // или любое начальное значение, которое вам нужно

            CatalogWindow.CartItems.Add(product); // Используем статическую переменную
            MessageBox.Show($"Товар {product.ProductName} добавлен в корзину");

            //if (UserSession.IsUserLoggedIn)
            //{
            //    var existingItem = CartItems.FirstOrDefault(item => item.ProductID == product.ProductID);
            //    if (existingItem != null)
            //    {
            //        if (existingItem.Quantity < product.AvailableQuantity)
            //        {
            //            existingItem.Quantity++;
            //        }
            //        else
            //        {
            //            MessageBox.Show("Невозможно добавить больше товара в корзину, чем имеется в наличии.");
            //        }
            //    }
            //    else
            //    {
            //        product.Quantity = 1;
            //        CartItems.Add(product);
            //        MessageBox.Show($"Товар {product.ProductName} добавлен в корзину");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Пожалуйста, войдите в свой профиль, чтобы добавить товар в корзину.");
            //}
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
