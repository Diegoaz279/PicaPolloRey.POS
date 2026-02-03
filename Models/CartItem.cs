using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PicaPolloRey.POS.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public Product Product { get; set; } = new Product();

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1) value = 1;
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineTotal));
            }
        }

        public decimal UnitPrice => Product.Price;
        public decimal LineTotal => UnitPrice * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
