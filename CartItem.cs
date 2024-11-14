using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinyl_store
{
    public static class Cart
    {
        public static List<CartItem> Items { get; private set; } = new List<CartItem>();

        public static void AddItem(CatalogProduct product)
        {
            var existingItem = Items.FirstOrDefault(item => item.Product.ProductID == product.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Items.Add(new CartItem { Product = product, Quantity = 1 });
            }
        }

        public static void RemoveItem(CatalogProduct product)
        {
            var existingItem = Items.FirstOrDefault(item => item.Product.ProductID == product.ProductID);
            if (existingItem != null)
            {
                Items.Remove(existingItem);
            }
        }

        public static void Clear()
        {
            Items.Clear();
        }

        public static decimal GetTotalPrice()
        {
            return Items.Sum(item => item.Product.ProductPrice * item.Quantity);
        }
    }
}
