using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class PaymentMethod
    {
        [Key]
        public int IdPaymentMethod { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public string Type { get; set; }  // "credit_card", "debit_card", "paypal"
        public string? Provider { get; set; }
        public string? AccountNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}