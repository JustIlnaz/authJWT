namespace authJWT.Requests
{
    public class UpdateProfileRequest
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AdressDelivery { get; set; }
        public string? Password { get; set; }
    }
}
