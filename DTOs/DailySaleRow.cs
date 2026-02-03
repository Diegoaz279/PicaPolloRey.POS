namespace PicaPolloRey.POS.DTOs
{
    public class DailySaleRow
    {
        public long Id { get; set; }
        public string Hora { get; set; } = "";
        public string MetodoPago { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal Itbis { get; set; }
        public decimal Total { get; set; }
    }
}
