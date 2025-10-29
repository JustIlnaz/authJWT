using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        public string? Email {  get; set; }
      
        public string? FullName { get; set; }
        public string? Phone { get; set; }

        public DateTime? CreatedDate { get; set; } = default(DateTime?);
        public DateTime? UpdateDate { get; set; } = default(DateTime?);


        [Required]
        [ForeignKey("Role")]

        public int RoleId { get; set; }
        public Role Roles { get; set; }


    }

}
