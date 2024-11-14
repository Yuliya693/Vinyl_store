using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
using static Vinyl_store.LoginWindow;

namespace Vinyl_store

{
    public partial class CatalogWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        public static List<CatalogProduct> CartItems { get; private set; } = new List<CatalogProduct>();

        public CatalogWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadProducts();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadComboBoxes()
        {
            LoadComboBox("SELECT GenreID, GenreName FROM Genre", GenreComboBox, "GenreID", "GenreName");
            LoadComboBox("SELECT ProductStatusID, ProductStatusName FROM ProductStatus", StatusComboBox, "ProductStatusID", "ProductStatusName");
            LoadComboBox("SELECT CountryID, CountryName FROM Country", CountryComboBox, "CountryID", "CountryName");
            LoadComboBox("SELECT StyleID, StyleName FROM Style", StyleComboBox, "StyleID", "StyleName");
        }

        private void LoadComboBox(string query, ComboBox comboBox, string valueMember, string displayMember)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox.Items.Add(new ComboBoxItem
                            {
                                Content = reader[displayMember].ToString(),
                                Tag = reader[valueMember]
                            });
                        }
                    }
                }
            }
        }

        private void LoadProducts(string searchQuery = "", string sortOption = "default")
        {
            List<CatalogProduct> products = new List<CatalogProduct>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT p.ProductID, p.ProductName, a.ArtistName AS ProductArtistName, p.ProductPrice, p.ProductPhoto, g.GenreName AS ProductGenre, 
                             ps.ProductStatusName AS ProductStatus, c.CountryName AS ProductCountry, s.StyleName AS ProductStyle, p.ProductCount AS AvailableQuantity
                             FROM Product p
                             JOIN Artist a ON p.ProductArtist = a.ArtistID
                             JOIN Genre g ON p.ProductGenre = g.GenreID
                             JOIN ProductStatus ps ON p.ProductStatus = ps.ProductStatusID
                             JOIN Country c ON p.ProductCountry = c.CountryID
                             JOIN Style s ON p.ProductStyle = s.StyleID
                             WHERE p.ProductName LIKE @searchQuery OR a.ArtistName LIKE @searchQuery";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new CatalogProduct
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
                                AvailableQuantity = int.Parse(reader["AvailableQuantity"].ToString())
                            };
                            products.Add(product);
                        }
                    }
                }
            }

            // Apply filters
            var filteredProducts = products.AsQueryable();
            if (GenreComboBox.SelectedItem != null && ((ComboBoxItem)GenreComboBox.SelectedItem).Tag != null)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductGenre == ((ComboBoxItem)GenreComboBox.SelectedItem).Content.ToString());
            }
            if (StatusComboBox.SelectedItem != null && ((ComboBoxItem)StatusComboBox.SelectedItem).Tag != null)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductStatus == ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString());
            }
            if (CountryComboBox.SelectedItem != null && ((ComboBoxItem)CountryComboBox.SelectedItem).Tag != null)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductCountry == ((ComboBoxItem)CountryComboBox.SelectedItem).Content.ToString());
            }
            if (StyleComboBox.SelectedItem != null && ((ComboBoxItem)StyleComboBox.SelectedItem).Tag != null)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductStyle == ((ComboBoxItem)StyleComboBox.SelectedItem).Content.ToString());
            }

            // Apply sorting
            switch (sortOption)
            {
                case "price_asc":
                    filteredProducts = filteredProducts.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.ProductPrice);
                    break;
                case "name_asc":
                    filteredProducts = filteredProducts.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.ProductName);
                    break;
                case "newest":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.ProductID);
                    break;
                case "oldest":
                    filteredProducts = filteredProducts.OrderBy(p => p.ProductID);
                    break;
            }

            ProductsWrapPanel.Children.Clear();
            foreach (var product in filteredProducts)
            {
                ProductsWrapPanel.Children.Add(CreateProductCard(product));
            }

            // Update results count
            ResultsCountTextBlock.Text = $"Найдено: {filteredProducts.Count()}";
            ResultsCountTextBlock.Visibility = filteredProducts.Any() ? Visibility.Visible : Visibility.Collapsed;

            UpdateSelectedFilters();
        }

        private void UpdateSelectedFilters()
        {
            SelectedFiltersPanel.Children.Clear();
            bool anyFilterSelected = false;

            if (GenreComboBox.SelectedItem != null && ((ComboBoxItem)GenreComboBox.SelectedItem).Tag != null)
            {
                
                AddFilterTag("Жанр", GenreComboBox.SelectedItem);
                anyFilterSelected = true;
            }
            if (StatusComboBox.SelectedItem != null && ((ComboBoxItem)StatusComboBox.SelectedItem).Tag != null)
            {
                AddFilterTag("Статус", StatusComboBox.SelectedItem);
                anyFilterSelected = true;
            }
            if (CountryComboBox.SelectedItem != null && ((ComboBoxItem)CountryComboBox.SelectedItem).Tag != null)
            {
                AddFilterTag("Страна", CountryComboBox.SelectedItem);
                anyFilterSelected = true;
            }
            if (StyleComboBox.SelectedItem != null && ((ComboBoxItem)StyleComboBox.SelectedItem).Tag != null)
            {
                AddFilterTag("Стиль", StyleComboBox.SelectedItem);
                anyFilterSelected = true;
            }

            ClearFiltersButton.Visibility = anyFilterSelected ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddFilterTag(string category, object selectedItem)
        {
            var filterTag = new TextBlock
            {
                Text = $"{((ComboBoxItem)selectedItem).Content.ToString()}",
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray)
            };

            var removeButton = new Button
            {
                Content = "x",
                Margin = new Thickness(5),
                Tag = selectedItem,
                Padding = new Thickness(2),
                Width = 20,
                Height = 20
            };
            removeButton.Click += (s, e) =>
            {
                switch (category)
                {
                    case "Жанр":
                        GenreComboBox.SelectedItem = null;
                        break;
                    case "Статус":
                        StatusComboBox.SelectedItem = null;
                        break;
                    case "Страна":
                        CountryComboBox.SelectedItem = null;
                        break;
                    case "Стиль":
                        StyleComboBox.SelectedItem = null;
                        break;
                }
                LoadProducts(SearchTextBox.Text, (SortComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
            };

            var filterPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            filterPanel.Children.Add(filterTag);
            filterPanel.Children.Add(removeButton);
            SelectedFiltersPanel.Children.Add(filterPanel);
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            GenreComboBox.SelectedItem = null;
            StatusComboBox.SelectedItem = null;
            CountryComboBox.SelectedItem = null;
            StyleComboBox.SelectedItem = null;
            LoadProducts(SearchTextBox.Text, (SortComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
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

        private UIElement CreateProductCard(CatalogProduct product)
        {
            var grid = new Grid
            {
                Width = 200,
                Height = 300,
                Margin = new Thickness(10)
            };

            var image = new Image
            {
                Source = product.ProductPhoto,
                Height = 100,
                Margin = new Thickness(10)
            };

            var nameTextBlock = new TextBlock
            {
                Text = $"{product.ProductArtistName} - {product.ProductName}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            };

            var priceTextBlock = new TextBlock
            {
                Text = $"{product.ProductPrice} р.",
                Margin = new Thickness(10)
            };

            var detailsButton = new Button
            {
                Content = "ПОДРОБНЕЕ",
                Width = 100,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Colors.White), // Цвет фона
                BorderBrush = new SolidColorBrush(Colors.Black), // Цвет обводки
                Foreground = new SolidColorBrush(Colors.Black), // Цвет текста
                FontSize = 13, // Размер шрифта
                /*FontWeight = FontWeights.Bold,*/ // Толщина шрифта
                FontFamily = new FontFamily("Arial") // Шрифт
            };

            var addToCartButton = new Button
            {
                Content = "В КОРЗИНУ",
                Width = 100,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Colors.Black), // Цвет фона
                BorderBrush = new SolidColorBrush(Colors.Black), // Цвет обводки
                Foreground = new SolidColorBrush(Colors.White), // Цвет текста
                FontSize = 13, // Размер шрифта
                // Толщина шрифта
                FontFamily = new FontFamily("Arial") // Шрифт
            };

            detailsButton.Click += (sender, e) => ShowProductDetails(product);
            addToCartButton.Click += (sender, e) => AddToCart(product);

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(nameTextBlock);
            stackPanel.Children.Add(priceTextBlock);
            stackPanel.Children.Add(detailsButton);
            stackPanel.Children.Add(addToCartButton);

            grid.Children.Add(stackPanel);
            return grid;
        }

        private void ShowProductDetails(CatalogProduct product)
        {
            var productDetailsWindow = new ProductDetailsWindow(product.ProductID);
            productDetailsWindow.ShowDialog();
        }

        private void AddToCart(CatalogProduct product)
        {
            if (UserSession.IsUserLoggedIn)
            {
                var existingItem = CartItems.FirstOrDefault(item => item.ProductID == product.ProductID);
                if (existingItem != null)
                {
                    if (existingItem.Quantity < product.AvailableQuantity)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        MessageBox.Show("Невозможно добавить больше товара в корзину, чем имеется в наличии.");
                    }
                }
                else
                {
                    product.Quantity = 1;
                    CartItems.Add(product);
                    MessageBox.Show($"Товар {product.ProductName} добавлен в корзину");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в свой профиль, чтобы добавить товар в корзину.");
            }

           
            
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text, (SortComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text, (SortComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text, (SortComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsUserLoggedIn)
            {
                var cartWindow = new CartWindow();
                cartWindow.Show();
            }
            else
            {
                MessageBox.Show("Пожалуйста, войдите в свой профиль, чтобы просмотреть корзину.");
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsUserLoggedIn)
            {
                var userProfileWindow = new UserProfileWindow();
                userProfileWindow.Show();
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            this.Close();
        }
    }


}
