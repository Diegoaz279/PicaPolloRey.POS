using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using PicaPolloRey.POS.DTOs;
using PicaPolloRey.POS.Services;

namespace PicaPolloRey.POS.ViewModels
{
    public partial class DailyReportViewModel : ObservableObject
    {
        private readonly SalesService _service;

        public ObservableCollection<DailySaleRow> Sales { get; } = new();

        [ObservableProperty] private decimal totalDia;
        [ObservableProperty] private decimal totalEfectivo;
        [ObservableProperty] private decimal totalTarjeta;

        public string TotalDiaText => TotalDia.ToString("C", CultureInfo.CurrentCulture);
        public string TotalEfectivoText => TotalEfectivo.ToString("C", CultureInfo.CurrentCulture);
        public string TotalTarjetaText => TotalTarjeta.ToString("C", CultureInfo.CurrentCulture);

        public DailyReportViewModel(SalesService service)
        {
            _service = service;
            Load();
        }

        private void Load()
        {
            var today = DateTime.Now;

            Sales.Clear();
            foreach (var s in _service.GetTodaySales(today))
                Sales.Add(s);

            var (td, te, tt) = _service.GetTodayTotals(today);
            TotalDia = td;
            TotalEfectivo = te;
            TotalTarjeta = tt;

            OnPropertyChanged(nameof(TotalDiaText));
            OnPropertyChanged(nameof(TotalEfectivoText));
            OnPropertyChanged(nameof(TotalTarjetaText));
        }

        [RelayCommand]
        private void Refresh() => Load();
    }
}
