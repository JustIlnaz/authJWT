using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class ShippingMethod
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
