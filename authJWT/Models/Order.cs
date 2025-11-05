using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Order
    {
        [Key]
        public int IdOrder { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [ForeignKey("Status")]
        public int StatusId { get; set; }
        public Status Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string? DeliveryMethod { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? PaymentMethod { get; set; }

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? Notes { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}