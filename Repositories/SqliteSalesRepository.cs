using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using PicaPolloRey.POS.Data;
using PicaPolloRey.POS.DTOs;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Repositories
{
    public class SqliteSalesRepository : ISalesRepository
    {
        private static decimal ScalarToDecimal(object? value)
        {
            if (value == null || value == DBNull.Value) return 0m;

            if (value is long l) return l;
            if (value is int i) return i;
            if (value is double d) return Convert.ToDecimal(d, CultureInfo.InvariantCulture);

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public long InsertSale(DateTime date, string metodoPago, decimal subtotal, decimal itbis, decimal total, List<CartItem> items)
        {
            using var conn = new SqliteConnection(DbConfig.ConnectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            long ventaId;
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO Venta (Fecha, MetodoPago, Subtotal, Itbis, Total)
VALUES ($fecha, $metodo, $subtotal, $itbis, $total);
SELECT last_insert_rowid();
";
                cmd.Parameters.AddWithValue("$fecha", date.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$metodo", metodoPago);
                cmd.Parameters.AddWithValue("$subtotal", (double)subtotal);
                cmd.Parameters.AddWithValue("$itbis", (double)itbis);
                cmd.Parameters.AddWithValue("$total", (double)total);

                ventaId = (long)cmd.ExecuteScalar()!;
            }

            foreach (var it in items)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO VentaDetalle (VentaId, ProductoId, NombreProducto, PrecioUnitario, Cantidad, TotalLinea)
VALUES ($ventaId, $productoId, $nombre, $precio, $cantidad, $totalLinea);
";
                cmd.Parameters.AddWithValue("$ventaId", ventaId);
                cmd.Parameters.AddWithValue("$productoId", it.Product.Id);
                cmd.Parameters.AddWithValue("$nombre", it.Product.Name);
                cmd.Parameters.AddWithValue("$precio", (double)it.UnitPrice);
                cmd.Parameters.AddWithValue("$cantidad", it.Quantity);
                cmd.Parameters.AddWithValue("$totalLinea", (double)it.LineTotal);

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return ventaId;
        }

        public List<DailySaleRow> GetTodaySales(DateTime today)
        {
            var list = new List<DailySaleRow>();

            var start = today.Date.ToString("yyyy-MM-dd 00:00:00");
            var end = today.Date.ToString("yyyy-MM-dd 23:59:59");

            using var conn = new SqliteConnection(DbConfig.ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT Id, Fecha, MetodoPago, Subtotal, Itbis, Total
FROM Venta
WHERE Fecha BETWEEN $start AND $end
ORDER BY Fecha DESC;
";
            cmd.Parameters.AddWithValue("$start", start);
            cmd.Parameters.AddWithValue("$end", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var fechaStr = reader.GetString(1);
                var hora = fechaStr.Length >= 16 ? fechaStr.Substring(11, 5) : fechaStr;

                list.Add(new DailySaleRow
                {
                    Id = reader.GetInt64(0),
                    Hora = hora,
                    MetodoPago = reader.GetString(2),
                    Subtotal = Convert.ToDecimal(reader.GetDouble(3), CultureInfo.InvariantCulture),
                    Itbis = Convert.ToDecimal(reader.GetDouble(4), CultureInfo.InvariantCulture),
                    Total = Convert.ToDecimal(reader.GetDouble(5), CultureInfo.InvariantCulture)
                });
            }

            return list;
        }

        public (decimal totalDia, decimal totalEfectivo, decimal totalTarjeta) GetTodayTotals(DateTime today)
        {
            var start = today.Date.ToString("yyyy-MM-dd 00:00:00");
            var end = today.Date.ToString("yyyy-MM-dd 23:59:59");

            using var conn = new SqliteConnection(DbConfig.ConnectionString);
            conn.Open();

            decimal totalDia;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT IFNULL(SUM(Total), 0)
FROM Venta
WHERE Fecha BETWEEN $start AND $end;
";
                cmd.Parameters.AddWithValue("$start", start);
                cmd.Parameters.AddWithValue("$end", end);

                totalDia = ScalarToDecimal(cmd.ExecuteScalar());
            }

            decimal totalEfectivo;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT IFNULL(SUM(Total), 0)
FROM Venta
WHERE Fecha BETWEEN $start AND $end AND MetodoPago = 'EFECTIVO';
";
                cmd.Parameters.AddWithValue("$start", start);
                cmd.Parameters.AddWithValue("$end", end);

                totalEfectivo = ScalarToDecimal(cmd.ExecuteScalar());
            }

            decimal totalTarjeta;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT IFNULL(SUM(Total), 0)
FROM Venta
WHERE Fecha BETWEEN $start AND $end AND MetodoPago = 'TARJETA';
";
                cmd.Parameters.AddWithValue("$start", start);
                cmd.Parameters.AddWithValue("$end", end);

                totalTarjeta = ScalarToDecimal(cmd.ExecuteScalar());
            }

            return (totalDia, totalEfectivo, totalTarjeta);
        }
    }
}
