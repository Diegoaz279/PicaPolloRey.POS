using Microsoft.Data.Sqlite;
using System.IO;

namespace PicaPolloRey.POS.Data
{
    public static class DbInitializer
    {
        public static void Initialize()
        {
            Directory.CreateDirectory(DbConfig.DbFolder);

            // ✅ Si ya existe la DB, NO correr schema.sql otra vez
            if (File.Exists(DbConfig.DbPath))
                return;

            using var conn = new SqliteConnection(DbConfig.ConnectionString);
            conn.Open();

            if (!File.Exists(DbConfig.SchemaPath))
                throw new FileNotFoundException("No se encontró db/schema.sql en el Output. Pon Copy always.", DbConfig.SchemaPath);

            var sql = File.ReadAllText(DbConfig.SchemaPath);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
