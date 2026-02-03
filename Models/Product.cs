namespace PicaPolloRey.POS.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public bool Active { get; set; } = true;

        public override string ToString() => $"{Name} - {Price:C}";
    }
}
