using System;
using System.Collections.Generic;
using PicaPolloRey.POS.DTOs;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Repositories
{
    public interface ISalesRepository
    {
        long InsertSale(DateTime date, string metodoPago, decimal subtotal, decimal itbis, decimal total, List<CartItem> items);
        List<DailySaleRow> GetTodaySales(DateTime today);
        (decimal totalDia, decimal totalEfectivo, decimal totalTarjeta) GetTodayTotals(DateTime today);
    }
}
