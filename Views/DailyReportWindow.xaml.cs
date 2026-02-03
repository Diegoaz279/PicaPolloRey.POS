using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PicaPolloRey.POS.Data;

namespace PicaPolloRey.POS.Views
{
    public partial class DailyReportWindow : Window
    {
        private readonly ReportState _state = new ReportState();

        public DailyReportWindow()
        {
            InitializeComponent();
            DataContext = _state;
            LoadReport();
        }

        private void LoadReport()
        {
            var today = DateTime.Now;
            var sales = PosDb.GetTodaySales(today);

            _state.Sales.Clear();
            foreach (var s in sales) _state.Sales.Add(s);

            var (totalDia, totalEfectivo, totalTarjeta) = PosDb.GetTodayTotals(today);
            _state.TotalDia = totalDia;
            _state.TotalEfectivo = totalEfectivo;
            _state.TotalTarjeta = totalTarjeta;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadReport();
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        public class ReportState : INotifyPropertyChanged
        {
            public ObservableCollection<PosDb.DailySaleRow> Sales { get; } = new ObservableCollection<PosDb.DailySaleRow>();

            private decimal _totalDia;
            private decimal _totalEfectivo;
            private decimal _totalTarjeta;

            public decimal TotalDia
            {
                get => _totalDia;
                set { _totalDia = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalDiaText)); }
            }

            public decimal TotalEfectivo
            {
                get => _totalEfectivo;
                set { _totalEfectivo = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalEfectivoText)); }
            }

            public decimal TotalTarjeta
            {
                get => _totalTarjeta;
                set { _totalTarjeta = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalTarjetaText)); }
            }

            public string TotalDiaText => $"{TotalDia:C}";
            public string TotalEfectivoText => $"{TotalEfectivo:C}";
            public string TotalTarjetaText => $"{TotalTarjeta:C}";

            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string? p = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }
    }
}
