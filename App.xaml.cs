using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using PicaPolloRey.POS.Data;
using PicaPolloRey.POS.Repositories;
using PicaPolloRey.POS.Services;
using PicaPolloRey.POS.Infrastructure;
using PicaPolloRey.POS.ViewModels;
using PicaPolloRey.POS.Views;

namespace PicaPolloRey.POS
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) DB init (no duplica)
            DbInitializer.Initialize();

            // 2) DI
            var services = new ServiceCollection();

            // Repos
            services.AddSingleton<IProductRepository, SqliteProductRepository>();
            services.AddSingleton<ISalesRepository, SqliteSalesRepository>();

            // Services
            services.AddSingleton<ProductService>();
            services.AddSingleton<SalesService>();

            // UI Infrastructure
            services.AddSingleton<IWindowService, WindowService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<DailyReportViewModel>();
            services.AddTransient<TicketViewModel>();

            Services = services.BuildServiceProvider();

            // 3) Main Window
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
    }
}
