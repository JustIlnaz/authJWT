using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Address
    {
        [Key]
        public int IdAddress { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string ZipCode { get; set; }
        public string? Country { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? AddressType { get; set; }  // "home", "work", "other"
    }
}