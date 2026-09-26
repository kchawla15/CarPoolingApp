namespace CarPoolingApp.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int RideId { get; set; }
        public string? PassengerId { get; set; }

        public DateTime BookingTime { get; set; }

        public string? PaymentMethod { get; set; }

        public Ride? Ride { get; set; }
    }
}