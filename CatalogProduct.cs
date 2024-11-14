using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Vinyl_store
{
    public class CatalogProduct
    {
        //public int ProductID { get; set; }
        //public string ProductName { get; set; }
        //public string ProductArtistName { get; set; }
        //public decimal ProductPrice { get; set; }
        //public BitmapImage ProductPhoto { get; set; }
        //public string ProductGenre { get; set; }
        //public string ProductStatus { get; set; }
        //public string ProductStyle { get; set; }
        //public string ProductCountry { get; set; }

        public int ProductID { get; set; }
        public BitmapImage ProductPhoto { get; set; }
        public string ProductName { get; set; }
        public string ProductArtistName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductGenre { get; set; }
        public string ProductStatus { get; set; }
        public string ProductCountry { get; set; }
        public string ProductStyle { get; set; }
        public string ProductDiscription { get; set; }
        public int Quantity { get; set; }
        public int ProductCount { get; set; }
                public int AvailableQuantity { get; set; } // Доступное количество на складе
    }
}
