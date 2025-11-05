using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class Category
    {
        [Key]
        public int IdCategory { get; set; }
        public string NameCategory { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Category> SubCategories { get; set; }
        public ICollection<Item> Items { get; set; }
    }
}