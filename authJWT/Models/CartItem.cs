using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class CartItem
    {
        [Key]
        public int IdCartItem { get; set; }

        [Required]
        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        [Required]
        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int Quantity { get; set; }
        public DateTime? AddedDate { get; set; } = DateTime.UtcNow;
    }
}