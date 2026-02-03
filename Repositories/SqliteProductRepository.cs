using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using PicaPolloRey.POS.Data;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Repositories
{
    public class SqliteProductRepository : IProductRepository
    {
        public List<Product> GetActive()
        {
            var list = new List<Product>();

            using var conn = new SqliteConnection(DbConfig.ConnectionString);
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

        public List<Product> GetAll()
        {
            var list = new List<Product>();

            using var conn = new SqliteConnection(DbConfig.ConnectionString);
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

        public long Insert(string name, string category, decimal price)
        {
            using var conn = new SqliteConnection(DbConfig.ConnectionString);
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

        public void Update(int id, string name, string category, decimal price)
        {
            using var conn = new SqliteConnection(DbConfig.ConnectionString);
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

        public void SetActive(int id, bool active)
        {
            using var conn = new SqliteConnection(DbConfig.ConnectionString);
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
    }
}
