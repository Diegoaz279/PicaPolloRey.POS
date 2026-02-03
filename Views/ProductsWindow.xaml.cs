using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using PicaPolloRey.POS.Data;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Views
{
    public partial class ProductsWindow : Window
    {
        private readonly ProductsState _state = new ProductsState();

        public ProductsWindow()
        {
            InitializeComponent();
            DataContext = _state;

            LoadProducts();
        }

        private void LoadProducts()
        {
            var list = PosDb.GetAllProducts();
            _state.Products.Clear();
            foreach (var p in list) _state.Products.Add(p);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadProducts();

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            _state.SelectedProduct = null;
            TxtName.Text = "";
            TxtCategory.Text = "";
            TxtPrice.Text = "";
            TxtName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = (TxtName.Text ?? "").Trim();
            var category = (TxtCategory.Text ?? "").Trim();
            var priceText = (TxtPrice.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show("La categoría es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                !decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.CurrentCulture, out price))
            {
                MessageBox.Show("Precio inválido. Ej: 120 o 120.50", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (price <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si hay seleccionado → UPDATE, si no → INSERT
            if (_state.SelectedProduct == null)
            {
                PosDb.InsertProduct(name, category, price);
                MessageBox.Show("Producto agregado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                PosDb.UpdateProduct(_state.SelectedProduct.Id, name, category, price);
                MessageBox.Show("Producto actualizado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadProducts();
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (_state.SelectedProduct == null)
            {
                MessageBox.Show("Selecciona un producto.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newState = !_state.SelectedProduct.Active;
            PosDb.SetProductActive(_state.SelectedProduct.Id, newState);

            LoadProducts();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        // Cuando seleccionas un producto en el grid, llena los textbox
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // no usado
        }

        private void ProductsGridSelectionChanged()
        {
            // no usado
        }

        // Hook simple cuando cambie SelectedProduct (en código)
        public class ProductsState : INotifyPropertyChanged
        {
            private Product? _selectedProduct;

            public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

            public Product? SelectedProduct
            {
                get => _selectedProduct;
                set
                {
                    _selectedProduct = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string? p = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        // Cuando seleccionan fila, llenamos campos (evento WPF)
        protected override void OnActivated(System.EventArgs e)
        {
            base.OnActivated(e);

            if (_state.SelectedProduct != null)
            {
                TxtName.Text = _state.SelectedProduct.Name;
                TxtCategory.Text = _state.SelectedProduct.Category;
                TxtPrice.Text = _state.SelectedProduct.Price.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
