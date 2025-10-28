using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Login
    {
        [Key]
        public int IdLogin { get; set; }

        public string LoginT { get; set; }

        public string Password { get; set; }

        [Required]
        [ForeignKey("User")]
       
        public int UserId {  get; set; }
        public User Users { get; set; }


    }
}
