using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Order
    {
      
        [Key]
        public int IdOrder { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryMethod { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }

        [Required]
        [ForeignKey("IdOrderItem")]
        public int OrderItemId { get; set; }
        public OrderItem OrderItems { get; set; }

        [Required]
        [ForeignKey("IdStatus")]

        public int StatusId { get; set; }
        public Status Statuses
    }
}
