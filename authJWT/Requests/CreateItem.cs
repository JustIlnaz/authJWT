namespace authJWT.Requests
{
    public class CreateItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; } = decimal.Zero;
        public decimal Count { get; set; } = decimal.Zero;
        public string Category { get; set; } = string.Empty;
    }
}
