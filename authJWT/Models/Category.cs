using System.ComponentModel.DataAnnotations;

namespace authJWT.Models
{
    public class Category
    {
        [Key]
        public int IdCategory { get; set; }
        public string NameCategory { get; set; }
    }
}
