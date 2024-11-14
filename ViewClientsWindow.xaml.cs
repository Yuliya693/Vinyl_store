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
    /// Логика взаимодействия для ViewClientsWindow.xaml
    /// </summary>
    public partial class ViewClientsWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public ViewClientsWindow()
        {
            InitializeComponent();
            LoadClients();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void LoadClients()
        {
            List<Client> clients = new List<Client>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT UserName, UserEmail, UserPhone, UserAddress 
                                 FROM Users 
                                 WHERE UserRole <> 2"; // Исключаем администраторов
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new Client
                            {
                                UserName = reader["UserName"].ToString(),
                                UserEmail = reader["UserEmail"].ToString(),
                                UserPhone = reader["UserPhone"].ToString(),
                                UserAddress = reader["UserAddress"].ToString()
                            };
                            clients.Add(client);
                        }
                    }
                }
            }

            ClientsListView.ItemsSource = clients;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class Client
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }
    }
}
