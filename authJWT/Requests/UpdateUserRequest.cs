namespace authJWT.Requests
{
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AdressDelivery { get; set; }
        public string? Role { get; set; }
    }
}
