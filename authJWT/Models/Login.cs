using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Login
    {
        [Key]
        public int IdLogin { get; set; }

        public string LoginT { get; set; }  = string.Empty;

        public string Password { get; set; } = string.Empty;

        [Required]
        [ForeignKey("User")]

        public int UserId {  get; set; }
        public User Users { get; set; }


    }
}
