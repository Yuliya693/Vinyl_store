using iTextSharp.text.pdf;
using iTextSharp.text;
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
using WpfParagraph = System.Windows.Documents.Paragraph;

namespace Vinyl_store
{
    public partial class SalesReportWindow : Window
    {
        private string connectionString = "Data Source=|DataDirectory|vinyl_store.sqlite;Version=3;";

        public SalesReportWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void SalesDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SalesDatePicker.SelectedDate != null)
            {
                LoadSalesReport(SalesDatePicker.SelectedDate.Value);
            }
        }

        private void LoadSalesReport(DateTime selectedDate)
        {
            List<SalesReportItem> salesReportItems = new List<SalesReportItem>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT p.ProductName, oi.OrderItemCount AS Quantity, oi.OrderItemPrice AS ProductPrice, (oi.OrderItemCount * oi.OrderItemPrice) AS TotalPrice
                                 FROM OrderItems oi
                                 JOIN Orders o ON oi.Order_ID = o.OrderID
                                 JOIN Product p ON oi.Product_ID = p.ProductID
                                 WHERE DATE(o.OrderDateTime) = @SelectedDate AND o.OrderStatus = 2"; // Assuming 2 is the status ID for "Выдан"
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SelectedDate", selectedDate.ToString("yyyy-MM-dd"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new SalesReportItem
                            {
                                ProductName = reader["ProductName"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ProductPrice = Convert.ToDecimal(reader["ProductPrice"]),
                                TotalPrice = Convert.ToDecimal(reader["TotalPrice"])
                            };
                            salesReportItems.Add(item);
                        }
                    }
                }
            }

            SalesListView.ItemsSource = salesReportItems;
            TotalSalesAmountTextBlock.Text = $"Общая сумма продаж: {salesReportItems.Sum(item => item.TotalPrice)} р.";
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDatePicker.SelectedDate != null)
            {
                SaveSalesReportAsPDF(SalesDatePicker.SelectedDate.Value);
            }
            else
            {
                MessageBox.Show("Выберите дату для создания отчета.");
            }
        }

        private void SaveSalesReportAsPDF(DateTime selectedDate)
        {
            List<SalesReportItem> salesReportItems = SalesListView.ItemsSource.Cast<SalesReportItem>().ToList();
            if (salesReportItems.Count == 0)
            {
                MessageBox.Show("Нет данных для отчета на выбранную дату.");
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Отчет_по_продажам_{selectedDate.ToString("yyyyMMdd")}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                document.Open();

                document.Add(new iTextSharp.text.Paragraph($"Отчет по продажам за {selectedDate.ToString("dd.MM.yyyy")}", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD)));
                document.Add(new iTextSharp.text.Paragraph("\n"));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Товар");
                table.AddCell("Количество");
                table.AddCell("Цена за единицу");
                table.AddCell("Общая цена");

                foreach (var item in salesReportItems)
                {
                    table.AddCell(item.ProductName);
                    table.AddCell(item.Quantity.ToString());
                    table.AddCell(item.ProductPrice.ToString("F2"));
                    table.AddCell(item.TotalPrice.ToString("F2"));
                }

                document.Add(table);

                document.Add(new iTextSharp.text.Paragraph($"\nОбщая сумма продаж: {salesReportItems.Sum(item => item.TotalPrice)} р.", new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)));

                document.Close();
                MessageBox.Show("Отчет успешно сохранен.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class SalesReportItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

