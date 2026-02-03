using System;
using System.Collections.Generic;
using PicaPolloRey.POS.Models;
using PicaPolloRey.POS.ViewModels;

namespace PicaPolloRey.POS.Infrastructure
{
    public interface IWindowService
    {
        void ShowProductsDialog();
        void ShowDailyReportDialog();
        void ShowTicketDialog(TicketViewModel vm);
    }
}
