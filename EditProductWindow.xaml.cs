using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        private Product currentProduct;

        public EditProductWindow(Product product)
        {
            InitializeComponent();
            currentProduct = product;
            LoadComboBoxes();
            LoadProductData();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadComboBoxes()
        {
            LoadComboBox("SELECT GenreID, GenreName FROM Genre", ProductGenreComboBox);
            LoadComboBox("SELECT ProductStatusID, ProductStatusName FROM ProductStatus", ProductStatusComboBox);
            LoadComboBox("SELECT StyleID, StyleName FROM Style", ProductStyleComboBox);
            LoadComboBox("SELECT CountryID, CountryName FROM Country", ProductCountryComboBox);
        }

        private void LoadComboBox(string query, ComboBox comboBox)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var items = new List<KeyValuePair<int, string>>();
                        while (reader.Read())
                        {
                            items.Add(new KeyValuePair<int, string>(Convert.ToInt32(reader[0]), reader[1].ToString()));
                        }
                        comboBox.ItemsSource = items;
                    }
                }
            }
        }

        private void LoadProductData()
        {
            ProductNameTextBox.Text = currentProduct.ProductName;
            ProductArtistTextBox.Text = currentProduct.ProductArtistName;
            ProductPriceTextBox.Text = currentProduct.ProductPrice.ToString();
            ProductYearTextBox.Text = currentProduct.ProductYear.ToString();
            ProductDescriptionTextBox.Text = currentProduct.ProductDescription;
            ProductCountTextBox.Text = currentProduct.ProductCount.ToString();
            ProductPhotoTextBox.Text = ""; // Add logic to convert image to URL if needed

            SetComboBoxValue(ProductGenreComboBox, currentProduct.ProductGenre);
            SetComboBoxValue(ProductStatusComboBox, currentProduct.ProductStatus);
            SetComboBoxValue(ProductStyleComboBox, currentProduct.ProductStyle);
            SetComboBoxValue(ProductCountryComboBox, currentProduct.ProductCountry);
        }

        private void SetComboBoxValue(ComboBox comboBox, string value)
        {
            foreach (KeyValuePair<int, string> item in comboBox.Items)
            {
                if (item.Value == value)
                {
                    comboBox.SelectedValue = item.Key;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string productName = ProductNameTextBox.Text;
            string productArtist = ProductArtistTextBox.Text;
            int? productGenre = ProductGenreComboBox.SelectedValue as int?;
            decimal productPrice = decimal.Parse(ProductPriceTextBox.Text);
            int productYear = int.Parse(ProductYearTextBox.Text);
            string productDescription = ProductDescriptionTextBox.Text;
            int productCount = int.Parse(ProductCountTextBox.Text);
            int? productStatus = ProductStatusComboBox.SelectedValue as int?;
            int? productStyle = ProductStyleComboBox.SelectedValue as int?;
            int? productCountry = ProductCountryComboBox.SelectedValue as int?;
            byte[] productPhoto = DownloadImage(ProductPhotoTextBox.Text);

            if (productGenre == null || productStatus == null || productStyle == null || productCountry == null)
            {
                MessageBox.Show("Please select a value for all dropdowns.");
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Check if artist exists and get artist ID, if not, insert new artist
                    int artistID = GetOrInsertArtist(connection, productArtist);

                    string query = @"UPDATE Product SET ProductName = @productName, ProductArtist = @productArtist, ProductGenre = @productGenre, 
                                     ProductPrice = @productPrice, ProductYear = @productYear, ProductDiscription = @productDescription, 
                                     ProductCount = @productCount, ProductStatus = @productStatus, ProductStyle = @productStyle, 
                                     ProductCountry = @productCountry, ProductPhoto = @productPhoto 
                                     WHERE ProductID = @productID";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@productName", productName);
                        command.Parameters.AddWithValue("@productArtist", artistID);
                        command.Parameters.AddWithValue("@productGenre", productGenre);
                        command.Parameters.AddWithValue("@productPrice", productPrice);
                        command.Parameters.AddWithValue("@productYear", productYear);
                        command.Parameters.AddWithValue("@productDescription", productDescription);
                        command.Parameters.AddWithValue("@productCount", productCount);
                        command.Parameters.AddWithValue("@productStatus", productStatus);
                        command.Parameters.AddWithValue("@productStyle", productStyle);
                        command.Parameters.AddWithValue("@productCountry", productCountry);
                        command.Parameters.AddWithValue("@productPhoto", productPhoto);
                        command.Parameters.AddWithValue("@productID", currentProduct.ProductID);
                        command.ExecuteNonQuery();
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private int GetOrInsertArtist(SQLiteConnection connection, string artistName)
        {
            // Check if artist exists
            string checkArtistQuery = "SELECT ArtistID FROM Artist WHERE ArtistName = @artistName";
            using (var command = new SQLiteCommand(checkArtistQuery, connection))
            {
                command.Parameters.AddWithValue("@artistName", artistName);
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            // Insert new artist
            string insertArtistQuery = "INSERT INTO Artist (ArtistName) VALUES (@artistName); SELECT last_insert_rowid()";
            using (var command = new SQLiteCommand(insertArtistQuery, connection))
            {
                command.Parameters.AddWithValue("@artistName", artistName);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private byte[] DownloadImage(string url)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    return webClient.DownloadData(url);
                }
            }
            catch
            {
                MessageBox.Show("Failed to download image.");
                return null;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            // Закрываем текущее окно
            this.Close();
        }
    }
}
