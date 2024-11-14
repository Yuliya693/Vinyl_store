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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";
        public LoginWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс сообщений об ошибках и обводки полей
            ResultTextBlock.Text = string.Empty;
            EmailInput.BorderBrush = Brushes.Gray;
            PasswordInput.BorderBrush = Brushes.Gray;

            string email = EmailInput.Text;
            string password = PasswordInput.Password;

            // Проверка пустых полей
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ResultTextBlock.Text = "Пожалуйста, заполните все поля.";
                if (string.IsNullOrEmpty(email))
                {
                    EmailInput.BorderBrush = Brushes.Red;
                }
                if (string.IsNullOrEmpty(password))
                {
                    PasswordInput.BorderBrush = Brushes.Red;
                }
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID, RoleName FROM Users INNER JOIN Role ON Users.UserRole = Role.RoleID WHERE UserEmail = @Email AND UserPassword = @Password";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["UserID"]);
                                string role = reader["RoleName"].ToString();

                                UserSession.IsUserLoggedIn = true;
                                UserSession.CurrentUserId = userId;

                                if (role == "Администратор")
                                {
                                    MessageBox.Show("Вы вошли под ролью администратора.");
                                    AdminMenu adminMenu = new AdminMenu();
                                    adminMenu.Show();
                                }
                                else
                                {
                                    CatalogWindow catalogWindow = new CatalogWindow();
                                    catalogWindow.Show();
                                }
                                this.Close();
                            }
                            else
                            {
                                ResultTextBlock.Text = "Такого профиля не существует.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "Error: " + ex.Message;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна регистрации
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
    public static class UserSession
    {
        public static bool IsUserLoggedIn { get; set; } = false;
        public static int CurrentUserId { get; set; } = -1;
    }
}
