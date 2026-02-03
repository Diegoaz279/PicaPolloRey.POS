using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using PicaPolloRey.POS.Helpers;
using PicaPolloRey.POS.Models;

namespace PicaPolloRey.POS.Views
{
    public partial class TicketWindow : Window
    {
        public TicketViewModel VM { get; }

        public TicketWindow(
            string ticketNumber,
            DateTime date,
            string paymentMethod,
            List<CartItem> cart,
            decimal subtotal,
            decimal itbis,
            decimal total,
            decimal itbisRate
        )
        {
            InitializeComponent();

            VM = new TicketViewModel(ticketNumber, date, paymentMethod, cart, subtotal, itbis, total, itbisRate);
            DataContext = VM;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class TicketLine
    {
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal Total { get; set; }
    }

    public class TicketViewModel
    {
        public string HeaderInfo { get; }
        public string PaymentInfo { get; }

        public ObservableCollection<TicketLine> Items { get; } = new ObservableCollection<TicketLine>();

        public decimal Subtotal { get; }
        public decimal Itbis { get; }
        public decimal Total { get; }

        public string SubtotalText => $"Subtotal: {Subtotal:C}";
        public string ItbisText => $"ITBIS: {Itbis:C}";
        public string TotalText => $"TOTAL: {Total:C}";

        public TicketViewModel(
            string ticketNumber,
            DateTime date,
            string paymentMethod,
            List<CartItem> cart,
            decimal subtotal,
            decimal itbis,
            decimal total,
            decimal itbisRate
        )
        {
            HeaderInfo = $"Ticket: {ticketNumber}  |  {date:dd/MM/yyyy HH:mm}";
            PaymentInfo = $"Pago: {paymentMethod}  |  ITBIS: {(itbisRate * 100):0}%";

            Subtotal = MoneyHelper.RoundMoney(subtotal);
            Itbis = MoneyHelper.RoundMoney(itbis);
            Total = MoneyHelper.RoundMoney(total);

            foreach (var c in cart)
            {
                Items.Add(new TicketLine
                {
                    Name = c.Product.Name,
                    Qty = c.Quantity,
                    Total = MoneyHelper.RoundMoney(c.LineTotal)
                });
            }
        }
    }
}
