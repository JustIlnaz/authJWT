namespace authJWT.Requests
{
    public class AddPaymentMethodRequest
    {
        public int CardNumber { get; set; }
        public string ExpiryDate { get; set; } = string.Empty;
        public int CodeCVC { get; set; }    
    }
}
