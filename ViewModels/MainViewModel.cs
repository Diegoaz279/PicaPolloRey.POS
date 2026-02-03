using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PicaPolloRey.POS.Infrastructure;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Services;

namespace PicaPolloRey.POS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const decimal ITBIS_RATE = 0.18m;

        private readonly ProductService _productService;
        private readonly SalesService _salesService;
        private readonly IWindowService _windowService;

        public ObservableCollection<Product> Products { get; } = new();
        public ICollectionView ProductsView { get; }

        public ObservableCollection<CartLineViewModel> Cart { get; } = new();

        [ObservableProperty] private Product? selectedProduct;
        [ObservableProperty] private CartLineViewModel? selectedCartItem;

        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private string todayText = "";

        [ObservableProperty] private bool isCash = true;

        [ObservableProperty] private decimal subtotal;
        [ObservableProperty] private decimal itbis;
        [ObservableProperty] private decimal total;

        public bool IsCard
        {
            get => !IsCash;
            set => IsCash = !value;
        }

        public string SubtotalText => Subtotal.ToString("C", CultureInfo.CurrentCulture);
        public string ItbisText => Itbis.ToString("C", CultureInfo.CurrentCulture);
        public string TotalText => Total.ToString("C", CultureInfo.CurrentCulture);

        public MainViewModel(ProductService productService, SalesService salesService, IWindowService windowService)
        {
            _productService = productService;
            _salesService = salesService;
            _windowService = windowService;

            TodayText = DateTime.Now.ToString("dddd, dd MMM yyyy - HH:mm", CultureInfo.CurrentCulture);

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProduct;

            Cart.CollectionChanged += Cart_CollectionChanged;

            LoadProducts();
            RecalculateTotals();
        }

        private bool FilterProduct(object obj)
        {
            if (obj is not Product p) return false;
            var q = (SearchText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q)) return true;
            return p.Name.Contains(q, StringComparison.InvariantCultureIgnoreCase)
                || p.Category.Contains(q, StringComparison.InvariantCultureIgnoreCase);
        }

        partial void OnSearchTextChanged(string value)
        {
            ProductsView.Refresh();
        }

        private void Cart_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (CartLineViewModel item in e.NewItems)
                    item.PropertyChanged += CartLine_PropertyChanged;

            if (e.OldItems != null)
                foreach (CartLineViewModel item in e.OldItems)
                    item.PropertyChanged -= CartLine_PropertyChanged;

            RecalculateTotals();
        }

        private void CartLine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartLineViewModel.Quantity) ||
                e.PropertyName == nameof(CartLineViewModel.LineTotal))
            {
                RecalculateTotals();
            }
        }

        private void LoadProducts()
        {
            Products.Clear();
            var list = _productService.GetActiveProducts();
            foreach (var p in list) Products.Add(p);

            ProductsView.Refresh();
        }

        private void RecalculateTotals()
        {
            decimal sub = 0m;
            foreach (var it in Cart)
                sub += it.LineTotal;

            Subtotal = sub;
            Itbis = sub * ITBIS_RATE;
            Total = Subtotal + Itbis;

            OnPropertyChanged(nameof(SubtotalText));
            OnPropertyChanged(nameof(ItbisText));
            OnPropertyChanged(nameof(TotalText));
        }

        [RelayCommand]
        private void AddToCart()
        {
            if (SelectedProduct == null) return;

            var existing = Cart.FirstOrDefault(x => x.Product.Id == SelectedProduct.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                Cart.Add(new CartLineViewModel(SelectedProduct, 1));
            }

            RecalculateTotals();
        }

        [RelayCommand]
        private void RemoveSelected()
        {
            if (SelectedCartItem == null) return;
            Cart.Remove(SelectedCartItem);
            SelectedCartItem = null;
            RecalculateTotals();
        }

        [RelayCommand]
        private void ClearCart()
        {
            if (Cart.Count == 0) return;
            Cart.Clear();
            SelectedCartItem = null;
            RecalculateTotals();
        }

        [RelayCommand]
        private void OpenProducts()
        {
            _windowService.ShowProductsDialog();
            LoadProducts(); // refrescar catálogo
        }

        [RelayCommand]
        private void OpenDailyReport()
        {
            _windowService.ShowDailyReportDialog();
        }

        public bool CanCharge => Cart.Count > 0;

        [RelayCommand(CanExecute = nameof(CanCharge))]
        private void Charge()
        {
            if (Cart.Count == 0) return;

            RecalculateTotals();

            var method = IsCash ? "EFECTIVO" : "TARJETA";

            // convertir CartLineVM a CartItem (modelo original) para guardar
            var items = Cart.Select(c => new CartItem
            {
                Product = c.Product,
                Quantity = c.Quantity
            }).ToList();

            long ventaId = _salesService.SaveSale(
                DateTime.Now,
                method,
                Subtotal,
                Itbis,
                Total,
                items
            );

            var ticketNumber = $"V-{ventaId}";

            // Ticket VM (copiamos los items para que no cambien si limpias carrito)
            var ticketItems = new ObservableCollection<CartLineViewModel>(
                Cart.Select(c => new CartLineViewModel(c.Product, c.Quantity)));

            var ticketVm = new TicketViewModel(ticketNumber, DateTime.Now, method, ticketItems, Subtotal, Itbis, Total, ITBIS_RATE);
            _windowService.ShowTicketDialog(ticketVm);

            Cart.Clear();
            SelectedCartItem = null;
            RecalculateTotals();
        }
    }
}
