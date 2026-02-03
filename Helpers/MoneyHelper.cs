namespace PicaPolloRey.POS.Helpers
{
    public static class MoneyHelper
    {
        public static decimal RoundMoney(decimal value) => decimal.Round(value, 2);
    }
}
