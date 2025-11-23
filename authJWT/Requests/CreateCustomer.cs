using System.ComponentModel.DataAnnotations;

namespace authJWT.Requests
{
    public class CreateCustomer
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Login_T{ get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string AdressDelivery { get; set; } = string.Empty;
        [Required]
        public int CardNumber { get; set; } = int.MaxValue;
        [Required]
        [StringLength(5, ErrorMessage = "Введен неверный формат")]
        public string ExpiryDate { get; set; } = string.Empty;
        [Required]
        public int CodeCVC { get; set; } = 0;
    }
}
    