using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class Status
    {
        [Key]
        public int IdStatus { get; set; }
        public string NameStatus { get; set; }  // "pending", "confirmed", "shipped", "delivered", "cancelled"
        public string? Description { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}