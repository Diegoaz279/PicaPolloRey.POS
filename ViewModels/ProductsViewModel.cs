using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Services;

namespace PicaPolloRey.POS.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly ProductService _service;

        public ObservableCollection<Product> Products { get; } = new();

        [ObservableProperty] private Product? selectedProduct;

        [ObservableProperty] private string name = "";
        [ObservableProperty] private string category = "";
        [ObservableProperty] private string priceText = "";

        public ProductsViewModel(ProductService service)
        {
            _service = service;
            Load();
        }

        private void Load()
        {
            Products.Clear();
            foreach (var p in _service.GetAllProducts())
                Products.Add(p);
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            if (value == null)
            {
                Name = "";
                Category = "";
                PriceText = "";
                return;
            }

            Name = value.Name;
            Category = value.Category;
            PriceText = value.Price.ToString(CultureInfo.InvariantCulture);
        }

        [RelayCommand]
        private void Refresh() => Load();

        [RelayCommand]
        private void New()
        {
            SelectedProduct = null;
            Name = "";
            Category = "";
            PriceText = "";
        }

        [RelayCommand]
        private void Save()
        {
            var n = (Name ?? "").Trim();
            var c = (Category ?? "").Trim();
            var ptxt = (PriceText ?? "").Trim();

            if (string.IsNullOrWhiteSpace(n) || string.IsNullOrWhiteSpace(c))
                return;

            if (!decimal.TryParse(ptxt, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                !decimal.TryParse(ptxt, NumberStyles.Any, CultureInfo.CurrentCulture, out price))
                return;

            if (price <= 0) return;

            if (SelectedProduct == null)
            {
                _service.AddProduct(n, c, price);
            }
            else
            {
                _service.UpdateProduct(SelectedProduct.Id, n, c, price);
            }

            Load();

            // re-seleccionar por nombre (simple)
            SelectedProduct = Products.FirstOrDefault(x => x.Name == n && x.Category == c);
        }

        [RelayCommand]
        private void ToggleActive()
        {
            if (SelectedProduct == null) return;
            _service.ToggleActive(SelectedProduct.Id, !SelectedProduct.Active);
            Load();
        }
    }
}
