using System.ComponentModel.DataAnnotations;

namespace authJWT.Requests
{
    public class CreateCustomer
    {
        [Required]
        public string FullName { get; set; } 
        [Required]
        public string Email { get; set; } 
        [Required]
        public string Login_T{ get; set; }
        [Required]
        public string Password { get; set; } 
        [Required]
        public string Phone { get; set; } 
        [Required]
        public string AdressDelivery { get; set; } 
        [Required]
        public int CardNumber { get; set; } 
        [Required]
        public string ExpiryDate { get; set; } 
        [Required]
        public int CodeCVC { get; set; } = 0;
    }
}
    