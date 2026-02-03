using System.Collections.Generic;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Repositories;

namespace PicaPolloRey.POS.Services
{
    public class ProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public List<Product> GetActiveProducts() => _repo.GetActive();
        public List<Product> GetAllProducts() => _repo.GetAll();

        public long AddProduct(string name, string category, decimal price)
            => _repo.Insert(name, category, price);

        public void UpdateProduct(int id, string name, string category, decimal price)
            => _repo.Update(id, name, category, price);

        public void ToggleActive(int id, bool newActive)
            => _repo.SetActive(id, newActive);
    }
}
