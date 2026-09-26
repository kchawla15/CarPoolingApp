namespace CarPoolingApp.Models
{
    public class CheckoutViewModel
    {
        public int RideId { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public decimal PricePerSeat { get; set; }

        public string? PaymentMethod { get; set; }
    }
}