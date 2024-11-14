using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для AdminRegistrationWindow.xaml
    /// </summary>
    public partial class AdminRegistrationWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public AdminRegistrationWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс сообщений об ошибках и обводки полей
            ResultTextBlock.Text = string.Empty;
            NameInput.BorderBrush = Brushes.Gray;
            EmailInput.BorderBrush = Brushes.Gray;
            PhoneInput.BorderBrush = Brushes.Gray;
            PasswordInput.BorderBrush = Brushes.Gray;
            AddressInput.BorderBrush = Brushes.Gray;
            RoleComboBox.BorderBrush = Brushes.Gray;

            string name = NameInput.Text;
            string email = EmailInput.Text;
            string phone = PhoneInput.Text;
            string password = PasswordInput.Password;
            string address = AddressInput.Text;
            int userRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag != null ? int.Parse((RoleComboBox.SelectedItem as ComboBoxItem).Tag.ToString()) : 0;

            // Проверка пустых полей
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(address) || userRole == 0)
            {
                ResultTextBlock.Text = "Пожалуйста, заполните все поля.";
                if (string.IsNullOrEmpty(name)) NameInput.BorderBrush = Brushes.Red;
                if (string.IsNullOrEmpty(email)) EmailInput.BorderBrush = Brushes.Red;
                if (string.IsNullOrEmpty(phone)) PhoneInput.BorderBrush = Brushes.Red;
                if (string.IsNullOrEmpty(password)) PasswordInput.BorderBrush = Brushes.Red;
                if (string.IsNullOrEmpty(address)) AddressInput.BorderBrush = Brushes.Red;
                if (userRole == 0) RoleComboBox.BorderBrush = Brushes.Red;
                return;
            }

            // Проверка формата почты
            if (!IsValidEmail(email))
            {
                ResultTextBlock.Text = "Некорректный формат почты.";
                EmailInput.BorderBrush = Brushes.Red;
                return;
            }

            // Проверка формата телефона
            if (!IsValidPhone(phone))
            {
                ResultTextBlock.Text = "Некорректный формат телефона. Формат: +79998887766";
                PhoneInput.BorderBrush = Brushes.Red;
                return;
            }

            // Проверка формата имени
            if (!IsValidName(name))
            {
                ResultTextBlock.Text = "Имя и фамилия должны начинаться с большой буквы.";
                NameInput.BorderBrush = Brushes.Red;
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Users (UserName, UserEmail, UserPhone, UserPassword, UserAddress, UserRole) VALUES (@Name, @Email, @Phone, @Password, @Address, @UserRole)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@UserRole", userRole);

                        command.ExecuteNonQuery();

                        MessageBox.Show("Регистрация успешна!");
                        this.Close();
                    }
                }
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                LogError(ex);
                // Логирование для отладки
                Console.WriteLine($"SQLiteException: {ex.Message}");

                if (ex.Message.Contains("Users.UserEmail"))
                {
                    ResultTextBlock.Text = "Пользователь с такой почтой уже существует.";
                    EmailInput.BorderBrush = Brushes.Red;
                }
                else if (ex.Message.Contains("Users.UserPhone"))
                {
                    ResultTextBlock.Text = "Пользователь с таким телефоном уже существует.";
                    PhoneInput.BorderBrush = Brushes.Red;
                }
                else
                {
                    ResultTextBlock.Text = "Ошибка базы данных: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                ResultTextBlock.Text = "Ошибка: " + ex.Message;
            }
        }

        private void LogError(Exception ex)
        {
            string logFilePath = "error_log.txt";
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTime.Now}] {ex.GetType()}: {ex.Message}");
                writer.WriteLine(ex.StackTrace);
            }
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool IsValidPhone(string phone)
        {
            var phonePattern = @"^\+?[0-9]{10,13}$";
            return Regex.IsMatch(phone, phonePattern);
        }

        private bool IsValidName(string name)
        {
            var namePattern = @"^[A-ZА-Я][a-zа-я]+\s[A-ZА-Я][a-zа-я]+$";
            return Regex.IsMatch(name, namePattern);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }
    }


}
