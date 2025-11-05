using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authJWT.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }
        [StringLength(19, ErrorMessage = "Введен неверный формат")]
        public int CardNumber { get; set; }
        [StringLength(5, ErrorMessage = "Введен неверный формат")]
        public string ExpiryDate { get; set; }
        [StringLength(3, ErrorMessage = "Введен неверный формат")]
        public int CodeCVC { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}