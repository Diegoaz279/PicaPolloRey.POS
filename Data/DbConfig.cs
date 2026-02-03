using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace PicaPolloRey.POS.Data
{
    public static class DbConfig
    {
        public static readonly string DbFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db");

        public static readonly string DbPath =
            Path.Combine(DbFolder, "pica_pollo_rey_pos.db");

        public static string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = DbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        public static string SchemaPath =>
            Path.Combine(DbFolder, "schema.sql");
    }
}
