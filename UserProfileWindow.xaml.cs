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
using static Vinyl_store.LoginWindow;

namespace Vinyl_store
{
    /// <summary>
    /// Логика взаимодействия для UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow : Window
    {
        public UserProfileWindow()
        {
            InitializeComponent();
            LoadUserProfile();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadUserProfile()
        {
            using (var connection = new SQLiteConnection("Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;"))
            {
                connection.Open();
                string query = "SELECT UserName, UserEmail, UserPhone, UserAddress FROM Users WHERE UserID = @UserID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", UserSession.CurrentUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserNameTextBlock.Text = reader["UserName"].ToString().ToUpper();
                            UserEmailTextBlock.Text = reader["UserEmail"].ToString();
                            UserPhoneTextBlock.Text = reader["UserPhone"].ToString();
                            UserAddressTextBlock.Text = reader["UserAddress"].ToString();
                        }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var catalogWindow = new CatalogWindow();
            catalogWindow.Show();
            this.Close();
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
