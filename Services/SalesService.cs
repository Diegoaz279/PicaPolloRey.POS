using System;
using System.Collections.Generic;
using PicaPolloRey.POS.DTOs;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.Repositories;

namespace PicaPolloRey.POS.Services
{
    public class SalesService
    {
        private readonly ISalesRepository _repo;

        public SalesService(ISalesRepository repo)
        {
            _repo = repo;
        }

        public long SaveSale(DateTime date, string metodoPago, decimal subtotal, decimal itbis, decimal total, List<CartItem> items)
            => _repo.InsertSale(date, metodoPago, subtotal, itbis, total, items);

        public List<DailySaleRow> GetTodaySales(DateTime today)
            => _repo.GetTodaySales(today);

        public (decimal totalDia, decimal totalEfectivo, decimal totalTarjeta) GetTodayTotals(DateTime today)
            => _repo.GetTodayTotals(today);
    }
}
