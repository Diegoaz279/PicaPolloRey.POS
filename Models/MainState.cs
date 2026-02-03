using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PicaPolloRey.POS.Models
{
    public class MainState : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> FilteredProducts { get; } = new ObservableCollection<Product>();
        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();

        private Product? _selectedProduct;
        private CartItem? _selectedCartItem;

        private string _searchText = "";
        private string _todayText = "";

        private bool _isCash = true;

        private decimal _subtotal;
        private decimal _itbis;
        private decimal _total;

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        public CartItem? SelectedCartItem
        {
            get => _selectedCartItem;
            set { _selectedCartItem = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); RefreshFilteredProducts(); }
        }

        public string TodayText
        {
            get => _todayText;
            set { _todayText = value; OnPropertyChanged(); }
        }

        public bool IsCash
        {
            get => _isCash;
            set
            {
                _isCash = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCard));
            }
        }

        public bool IsCard
        {
            get => !_isCash;
            set
            {
                _isCash = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCash));
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        public decimal Itbis
        {
            get => _itbis;
            set { _itbis = value; OnPropertyChanged(); }
        }

        public decimal Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); }
        }

        public void RefreshFilteredProducts()
        {
            FilteredProducts.Clear();

            var q = (SearchText ?? "").Trim().ToLowerInvariant();
            var items = string.IsNullOrWhiteSpace(q)
                ? Products
                : new ObservableCollection<Product>(Products.Where(p => p.Name.ToLowerInvariant().Contains(q)));

            foreach (var p in items)
                FilteredProducts.Add(p);
        }

        public void RecalculateTotals(decimal itbisRate)
        {
            decimal subtotal = 0m;
            foreach (var it in Cart)
            {
                if (it.Quantity < 1) it.Quantity = 1;
                subtotal += it.LineTotal;
            }

            Subtotal = subtotal;
            Itbis = subtotal * itbisRate;
            Total = Subtotal + Itbis;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
