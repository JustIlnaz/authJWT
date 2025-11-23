using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // "pending", "confirmed", "shipped", "delivered", "cancelled" 
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}