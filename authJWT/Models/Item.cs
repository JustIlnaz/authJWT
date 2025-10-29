using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("CategoryId")]
        public int CategoryId {  get; set; }
        public Category Categories { get; set; }


    }
}
