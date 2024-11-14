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

using System.Data.SQLite;
using System.Net;
using System.Data;


namespace Vinyl_store
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public AddProductWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;

        }

        private void LoadComboBoxes()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Загрузка жанров
                var genreQuery = "SELECT GenreID, GenreName FROM Genre";
                var genreAdapter = new SQLiteDataAdapter(genreQuery, connection);
                var genreTable = new DataTable();
                genreAdapter.Fill(genreTable);
                ProductGenreComboBox.ItemsSource = genreTable.DefaultView;

                // Загрузка статусов
                var statusQuery = "SELECT ProductStatusID, ProductStatusName FROM ProductStatus";
                var statusAdapter = new SQLiteDataAdapter(statusQuery, connection);
                var statusTable = new DataTable();
                statusAdapter.Fill(statusTable);
                ProductStatusComboBox.ItemsSource = statusTable.DefaultView;

                // Загрузка стилей
                var styleQuery = "SELECT StyleID, StyleName FROM Style";
                var styleAdapter = new SQLiteDataAdapter(styleQuery, connection);
                var styleTable = new DataTable();
                styleAdapter.Fill(styleTable);
                ProductStyleComboBox.ItemsSource = styleTable.DefaultView;

                // Загрузка стран
                var countryQuery = "SELECT CountryID, CountryName FROM Country";
                var countryAdapter = new SQLiteDataAdapter(countryQuery, connection);
                var countryTable = new DataTable();
                countryAdapter.Fill(countryTable);
                ProductCountryComboBox.ItemsSource = countryTable.DefaultView;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string productName = ProductNameTextBox.Text;
            string productArtist = ProductArtistTextBox.Text;
            int productGenre = Convert.ToInt32(ProductGenreComboBox.SelectedValue);
            decimal productPrice = decimal.Parse(ProductPriceTextBox.Text);
            int productYear = int.Parse(ProductYearTextBox.Text);
            string productDescription = ProductDescriptionTextBox.Text;
            int productCount = int.Parse(ProductCountTextBox.Text);
            int productStatus = Convert.ToInt32(ProductStatusComboBox.SelectedValue);
            int productStyle = Convert.ToInt32(ProductStyleComboBox.SelectedValue);
            int productCountry = Convert.ToInt32(ProductCountryComboBox.SelectedValue);
            byte[] productPhoto = DownloadImage(ProductPhotoTextBox.Text);

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Добавление артиста и получение его ArtistID
                    int artistId = AddArtistAndGetId(connection, productArtist);

                    // Добавление товара с использованием полученного ArtistID
                    string insertQuery = @"INSERT INTO Product (ProductName, ProductArtist, ProductGenre, ProductPrice, ProductYear, ProductDiscription, ProductCount, ProductStatus, ProductStyle, ProductCountry, ProductPhoto)
                                           VALUES (@ProductName, @ProductArtist, @ProductGenre, @ProductPrice, @ProductYear, @ProductDiscription, @ProductCount, @ProductStatus, @ProductStyle, @ProductCountry, @ProductPhoto)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", productName);
                        command.Parameters.AddWithValue("@ProductArtist", artistId);
                        command.Parameters.AddWithValue("@ProductGenre", productGenre);
                        command.Parameters.AddWithValue("@ProductPrice", productPrice);
                        command.Parameters.AddWithValue("@ProductYear", productYear);
                        command.Parameters.AddWithValue("@ProductDiscription", productDescription);
                        command.Parameters.AddWithValue("@ProductCount", productCount);
                        command.Parameters.AddWithValue("@ProductStatus", productStatus);
                        command.Parameters.AddWithValue("@ProductStyle", productStyle);
                        command.Parameters.AddWithValue("@ProductCountry", productCountry);
                        command.Parameters.AddWithValue("@ProductPhoto", productPhoto);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Товар успешно добавлен!");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении товара: " + ex.Message);
            }
        }

        private int AddArtistAndGetId(SQLiteConnection connection, string artistName)
        {
            // Проверка, существует ли уже артист с таким именем
            string selectQuery = "SELECT ArtistID FROM Artist WHERE ArtistName = @ArtistName";
            using (var selectCommand = new SQLiteCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@ArtistName", artistName);
                var result = selectCommand.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            // Добавление нового артиста
            string insertQuery = "INSERT INTO Artist (ArtistName) VALUES (@ArtistName); SELECT last_insert_rowid();";
            using (var insertCommand = new SQLiteCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@ArtistName", artistName);
                return Convert.ToInt32(insertCommand.ExecuteScalar());
            }
        }

        private void ClearForm()
        {
            ProductNameTextBox.Clear();
            ProductArtistTextBox.Clear();
            ProductGenreComboBox.SelectedIndex = -1;
            ProductPriceTextBox.Clear();
            ProductYearTextBox.Clear();
            ProductDescriptionTextBox.Clear();
            ProductCountTextBox.Clear();
            ProductStatusComboBox.SelectedIndex = -1;
            ProductStyleComboBox.SelectedIndex = -1;
            ProductCountryComboBox.SelectedIndex = -1;
            ProductPhotoTextBox.Clear();
        }

        private byte[] DownloadImage(string url)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки изображения: " + ex.Message);
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
