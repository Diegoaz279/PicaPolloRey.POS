using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Data
{
    public static class PosDb
    {
        private static readonly string DbFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db");

        private static readonly string DbPath =
            Path.Combine(DbFolder, "pica_pollo_rey_pos.db");

        private static string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = DbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        // ==========================
        // INIT (NO duplica seed)
        // ==========================
        public static void Initialize()
        {
            Directory.CreateDirectory(DbFolder);

            // ✅ Si la DB ya existe, NO vuelvas a correr schema.sql (evita duplicados)
            if (File.Exists(DbPath))
                return;

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var schemaPath = Path.Combine(DbFolder, "schema.sql");
            if (!File.Exists(schemaPath))
                throw new FileNotFoundException("No se encontró db/schema.sql. Asegúrate de crearlo y poner Copy always.", schemaPath);

            var sql = File.ReadAllText(schemaPath);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        // ==========================
        // PRODUCTS
        // ==========================
        public static List<Product> GetActiveProducts()
        {
            var list = new List<Product>();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT Id, Nombre, Categoria, Precio, Activo
FROM Producto
WHERE Activo = 1
ORDER BY Categoria, Nombre;
";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = Convert.ToDecimal(reader.GetDouble(3), CultureInfo.InvariantCulture),
                    Active = reader.GetInt32(4) == 1
                });
            }

            return list;
        }

        public static List<Product> GetAllProducts()
        {
            var list = new List<Product>();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT Id, Nombre, Categoria, Precio, Activo
FROM Producto
ORDER BY Categoria, Nombre;
";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = Convert.ToDecimal(reader.GetDouble(3), CultureInfo.InvariantCulture),
                    Active = reader.GetInt32(4) == 1
                });
            }

            return list;
        }

        public static long InsertProduct(string name, string category, decimal price)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
VALUES ($nombre, $categoria, $precio, 1);
SELECT last_insert_rowid();
";
            cmd.Parameters.AddWithValue("$nombre", name);
            cmd.Parameters.AddWithValue("$categoria", category);
            cmd.Parameters.AddWithValue("$precio", (double)price);

            return (long)cmd.ExecuteScalar()!;
        }

        public static void UpdateProduct(int id, string name, string category, decimal price)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE Producto
SET Nombre = $nombre,
    Categoria = $categoria,
    Precio = $precio
WHERE Id = $id;
";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$nombre", name);
            cmd.Parameters.AddWithValue("$categoria", category);
            cmd.Parameters.AddWithValue("$precio", (double)price);

            cmd.ExecuteNonQuery();
        }

        public static void SetProductActive(int id, bool active)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE Producto
SET Activo = $activo
WHERE Id = $id;
";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$activo", active ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        // ==========================
        // SALES (insert)
        // ==========================
        public static long InsertSale(DateTime date, string metodoPago, decimal subtotal, decimal itbis, decimal total, List<CartItem> items)
        {
            using var conn = new SqliteConnection(ConnectionString);
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

        // ==========================
        // REPORT (today)
        // ==========================
        public class DailySaleRow
        {
            public long Id { get; set; }
            public string Hora { get; set; } = "";
            public string MetodoPago { get; set; } = "";
            public decimal Subtotal { get; set; }
            public decimal Itbis { get; set; }
            public decimal Total { get; set; }
        }

        public static List<DailySaleRow> GetTodaySales(DateTime today)
        {
            var list = new List<DailySaleRow>();

            var start = today.Date.ToString("yyyy-MM-dd 00:00:00");
            var end = today.Date.ToString("yyyy-MM-dd 23:59:59");

            using var conn = new SqliteConnection(ConnectionString);
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

        // ✅ Helper: convierte ExecuteScalar() sin explotar (Int64/Double/DBNull)
        private static decimal ScalarToDecimal(object? value)
        {
            if (value == null || value == DBNull.Value) return 0m;

            try
            {
                // SQLite puede devolver Int64 o Double según el caso
                if (value is long l) return l;
                if (value is int i) return i;
                if (value is double d) return Convert.ToDecimal(d, CultureInfo.InvariantCulture);
                if (value is float f) return Convert.ToDecimal(f, CultureInfo.InvariantCulture);
                if (value is decimal dec) return dec;

                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0m;
            }
        }

        public static (decimal totalDia, decimal totalEfectivo, decimal totalTarjeta) GetTodayTotals(DateTime today)
        {
            var start = today.Date.ToString("yyyy-MM-dd 00:00:00");
            var end = today.Date.ToString("yyyy-MM-dd 23:59:59");

            using var conn = new SqliteConnection(ConnectionString);
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
