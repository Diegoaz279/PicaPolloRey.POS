using System.Collections.Generic;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Repositories
{
    public interface IProductRepository
    {
        List<Product> GetActive();
        List<Product> GetAll();
        long Insert(string name, string category, decimal price);
        void Update(int id, string name, string category, decimal price);
        void SetActive(int id, bool active);
    }
}
