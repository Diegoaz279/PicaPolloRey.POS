using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PicaPolloRey.POS.Helpers;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Views;
using PicaPolloRey.POS.Data;

namespace PicaPolloRey.POS
{
    public partial class MainWindow : Window
    {
        // ITBIS: lo dejamos fijo en Commit 1 (luego lo hacemos configurable)
        private const decimal ITBIS_RATE = 0.18m;

        private readonly MainState _state = new MainState();

        public MainWindow()
        {
            InitializeComponent();

            _state.TodayText = DateTime.Now.ToString("dddd, dd MMM yyyy - HH:mm");
            PosDb.Initialize(); // crea BD + tablas + seed
            _state.LoadProductsFromDb();
            _state.RefreshFilteredProducts();
            _state.RecalculateTotals(ITBIS_RATE);

            DataContext = _state;
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (_state.SelectedProduct == null)
            {
                MessageBox.Show("Selecciona un producto primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var existing = _state.Cart.FirstOrDefault(x => x.Product.Id == _state.SelectedProduct.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _state.Cart.Add(new CartItem
                {
                    Product = _state.SelectedProduct,
                    Quantity = 1
                });
            }

            _state.RecalculateTotals(ITBIS_RATE);
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (_state.SelectedCartItem == null)
            {
                MessageBox.Show("Selecciona un item del carrito.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _state.Cart.Remove(_state.SelectedCartItem);
            _state.SelectedCartItem = null;
            _state.RecalculateTotals(ITBIS_RATE);
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_state.Cart.Count == 0)
                return;

            var res = MessageBox.Show("¿Vaciar el carrito?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            _state.Cart.Clear();
            _state.SelectedCartItem = null;
            _state.RecalculateTotals(ITBIS_RATE);
        }

        private void BtnCharge_Click(object sender, RoutedEventArgs e)
        {
            if (_state.Cart.Count == 0)
            {
                MessageBox.Show("No puedes cobrar una venta vacía.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var item in _state.Cart)
            {
                if (item.Quantity < 1)
                    item.Quantity = 1;
            }

            _state.RecalculateTotals(ITBIS_RATE);

            var paymentMethod = _state.IsCash ? "EFECTIVO" : "TARJETA";

            // Guardar en BD
            long ventaId = PosDb.InsertSale(
                DateTime.Now,
                paymentMethod,
                _state.Subtotal,
                _state.Itbis,
                _state.Total,
                _state.Cart.ToList()
            );

            var ticketNumber = $"V-{ventaId}";

            var ticket = new TicketWindow(
                ticketNumber,
                DateTime.Now,
                paymentMethod,
                _state.Cart.ToList(),
                _state.Subtotal,
                _state.Itbis,
                _state.Total,
                ITBIS_RATE
            );

            ticket.Owner = this;
            ticket.ShowDialog();

            _state.Cart.Clear();
            _state.SelectedCartItem = null;
            _state.RecalculateTotals(ITBIS_RATE);
        }
    }
    // Estado monolítico (Commit 1): todo en la ventana
    public class MainState : INotifyPropertyChanged
    {
        private string _todayText = "";
        private string _searchText = "";
        private Product? _selectedProduct;
        private CartItem? _selectedCartItem;

        private bool _isCash = true;
        private bool _isCard = false;

        private decimal _subtotal;
        private decimal _itbis;
        private decimal _total;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> FilteredProducts { get; } = new ObservableCollection<Product>();

        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();

        public string TodayText
        {
            get => _todayText;
            set { _todayText = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? "";
                OnPropertyChanged();
                RefreshFilteredProducts();
            }
        }

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

        public bool IsCash
        {
            get => _isCash;
            set
            {
                _isCash = value;
                if (value) _isCard = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCard));
            }
        }

        public bool IsCard
        {
            get => _isCard;
            set
            {
                _isCard = value;
                if (value) _isCash = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCash));
            }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubtotalText)); }
        }

        public decimal Itbis
        {
            get => _itbis;
            set { _itbis = value; OnPropertyChanged(); OnPropertyChanged(nameof(ItbisText)); }
        }

        public decimal Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalText)); }
        }

        public string SubtotalText => $"Subtotal: {Subtotal:C}";
        public string ItbisText => $"ITBIS: {Itbis:C}";
        public string TotalText => $"TOTAL: {Total:C}";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void LoadProductsFromDb()
        {
            Products.Clear();

            var dbProducts = PosDb.GetActiveProducts();
            foreach (var p in dbProducts)
                Products.Add(p);
        }

        public void RefreshFilteredProducts()
        {
            FilteredProducts.Clear();

            var text = (SearchText ?? "").Trim().ToLower();
            var list = string.IsNullOrWhiteSpace(text)
                ? Products.ToList()
                : Products.Where(p => p.Name.ToLower().Contains(text)).ToList();

            foreach (var p in list)
                FilteredProducts.Add(p);
        }

        public void RecalculateTotals(decimal itbisRate)
        {
            // Recalcular subtotal desde el carrito
            var subtotal = Cart.Sum(x => x.LineTotal);

            // redondeos
            Subtotal = MoneyHelper.RoundMoney(subtotal);
            Itbis = MoneyHelper.RoundMoney(Subtotal * itbisRate);
            Total = MoneyHelper.RoundMoney(Subtotal + Itbis);
        }

        private void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
