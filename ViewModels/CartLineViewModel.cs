using CommunityToolkit.Mvvm.ComponentModel;
using System;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.ViewModels
{
    public partial class CartLineViewModel : ObservableObject
    {
        public Product Product { get; }

        [ObservableProperty]
        private int quantity;

        public decimal UnitPrice => Product.Price;
        public decimal LineTotal => UnitPrice * Quantity;

        public CartLineViewModel(Product product, int qty = 1)
        {
            Product = product;
            quantity = Math.Max(1, qty);
        }

        partial void OnQuantityChanged(int value)
        {
            if (value < 1)
                Quantity = 1;

            OnPropertyChanged(nameof(LineTotal));
        }
    }
}
