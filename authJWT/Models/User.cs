using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace authJWT.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Roles { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public Cart Cart { get; set; }
    }
}