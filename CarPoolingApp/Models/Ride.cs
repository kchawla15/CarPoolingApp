using System;
using System.ComponentModel.DataAnnotations;

namespace CarPoolingApp.Models
{
    public class Ride
    {
        public int Id { get; set; }
        public string? RideId { get; set; }

        public string? DriverId { get; set; }

        [Required]
        public string? FromLocation { get; set; }

        [Required]
        public string? ToLocation { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        public int AvailableSeats { get; set; }

        public decimal PricePerSeat { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}