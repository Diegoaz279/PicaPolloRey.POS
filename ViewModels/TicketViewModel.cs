using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace PicaPolloRey.POS.ViewModels
{
    public partial class TicketViewModel : ObservableObject
    {
        public string TicketNumber { get; }
        public DateTime Date { get; }
        public string PaymentMethod { get; }

        public ObservableCollection<CartLineViewModel> Items { get; }

        public decimal Subtotal { get; }
        public decimal Itbis { get; }
        public decimal Total { get; }
        public decimal ItbisRate { get; }

        public TicketViewModel(
            string ticketNumber,
            DateTime date,
            string paymentMethod,
            ObservableCollection<CartLineViewModel> items,
            decimal subtotal,
            decimal itbis,
            decimal total,
            decimal itbisRate)
        {
            TicketNumber = ticketNumber;
            Date = date;
            PaymentMethod = paymentMethod;
            Items = items;

            Subtotal = subtotal;
            Itbis = itbis;
            Total = total;
            ItbisRate = itbisRate;
        }
    }
}
