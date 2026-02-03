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

        public static void Initialize()
        {
            Directory.CreateDirectory(DbFolder);

            // Asegurar que la base exista (ReadWriteCreate la crea si no existe)
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            // Ejecutar schema.sql
            var schemaPath = Path.Combine(DbFolder, "schema.sql");
            if (!File.Exists(schemaPath))
                throw new FileNotFoundException("No se encontró db/schema.sql. Asegúrate de crear el archivo.", schemaPath);

            var sql = File.ReadAllText(schemaPath);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

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

        public static long InsertSale(DateTime date, string metodoPago, decimal subtotal, decimal itbis, decimal total, List<CartItem> items)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            // Insert Venta
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

            // Insert Detalles
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
    }
}
