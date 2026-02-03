using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using PicaPolloRey.POS.ViewModels;
using PicaPolloRey.POS.Views;

namespace PicaPolloRey.POS.Infrastructure
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _sp;

        public WindowService(IServiceProvider sp)
        {
            _sp = sp;
        }

        public void ShowProductsDialog()
        {
            var vm = _sp.GetRequiredService<ProductsViewModel>();
            var w = new ProductsWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            w.ShowDialog();
        }

        public void ShowDailyReportDialog()
        {
            var vm = _sp.GetRequiredService<DailyReportViewModel>();
            var w = new DailyReportWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            w.ShowDialog();
        }

        public void ShowTicketDialog(TicketViewModel vm)
        {
            var w = new TicketWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            w.ShowDialog();
        }
    }
}
