using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class Item
    {
        [Key]
        public int IdItem { get; set; }
        public string NameItem { get; set; }
        public string DescriptionItem { get; set; }
        public double PriceItem { get; set; }
        public int StockItem { get; set; }


        



    }
}
