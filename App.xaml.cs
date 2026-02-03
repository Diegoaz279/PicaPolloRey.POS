using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using PicaPolloRey.POS.Data;
using PicaPolloRey.POS.Repositories;
using PicaPolloRey.POS.Services;

namespace PicaPolloRey.POS
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DbInitializer.Initialize();

            var services = new ServiceCollection();

            services.AddSingleton<IProductRepository, SqliteProductRepository>();
            services.AddSingleton<ISalesRepository, SqliteSalesRepository>();

            services.AddSingleton<ProductService>();
            services.AddSingleton<SalesService>();

            Services = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : notnull
            => Services.GetRequiredService<T>();
    }
}
