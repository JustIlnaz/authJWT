using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class Role
    {
        [Key]
        public int IdRole { get; set; }

        public string NameRole { get; set; }
    }
}
