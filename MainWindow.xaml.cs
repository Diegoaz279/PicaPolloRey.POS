using System;
using System.Linq;
using System.Windows;
using PicaPolloRey.POS.Helpers;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Services;
using PicaPolloRey.POS.Views;


namespace PicaPolloRey.POS
{
    public partial class MainWindow : Window
    {
        private const decimal ITBIS_RATE = 0.18m;

        private readonly MainState _state = new MainState();
        private readonly ProductService _productService;
        private readonly SalesService _salesService;


        public MainWindow()
        {
            InitializeComponent();

            _productService = App.GetService<ProductService>();
            _salesService = App.GetService<SalesService>();

            _state.TodayText = DateTime.Now.ToString("dddd, dd MMM yyyy - HH:mm");
            LoadProducts();

            _state.RecalculateTotals(ITBIS_RATE);

            DataContext = _state;
        }

        private void LoadProducts()
        {
            _state.Products.Clear();
            var list = _productService.GetActiveProducts();
            foreach (var p in list) _state.Products.Add(p);

            _state.RefreshFilteredProducts();
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
                existing.Quantity += 1;
            else
                _state.Cart.Add(new CartItem { Product = _state.SelectedProduct, Quantity = 1 });

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
            if (_state.Cart.Count == 0) return;

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
                if (item.Quantity < 1) item.Quantity = 1;

            _state.RecalculateTotals(ITBIS_RATE);

            var paymentMethod = _state.IsCash ? "EFECTIVO" : "TARJETA";

            long ventaId = _salesService.SaveSale(
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

        private void BtnOpenProducts_Click(object sender, RoutedEventArgs e)
        {
            var w = new ProductsWindow();
            w.Owner = this;
            w.ShowDialog();

            // recargar catálogo por si cambiaron cosas
            LoadProducts();
        }

        private void BtnOpenReport_Click(object sender, RoutedEventArgs e)
        {
            var w = new DailyReportWindow();
            w.Owner = this;
            w.ShowDialog();
        }
    }
}
