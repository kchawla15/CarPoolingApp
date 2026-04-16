using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarPoolingApp.Data;
using CarPoolingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CarPoolingApp.Services;

namespace CarPoolingApp.Controllers
{


    [Authorize]
    public class RidesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;


        public RidesController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Rides
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var rides = await _context.Rides
                .Where(r => r.DriverId == user.DriverNumber.ToString())
                .ToListAsync();

            return View(rides);
        }

        // GET: Rides/FindRide
        [AllowAnonymous]
        public async Task<IActionResult> FindRide(string fromLocation, string toLocation, DateTime? departureDate)
        {
            if (!departureDate.HasValue)
            {
                ViewBag.HasSearched = false;
                return View(new List<Ride>());
            }

            var rides = _context.Rides.AsQueryable();

            if (!string.IsNullOrEmpty(fromLocation))
            {
                rides = rides.Where(r => r.FromLocation != null && r.FromLocation.Contains(fromLocation));
            }

            if (!string.IsNullOrEmpty(toLocation))
            {
                rides = rides.Where(r => r.ToLocation != null && r.ToLocation.Contains(toLocation));
            }

            rides = rides.Where(r => r.DepartureTime.Date == departureDate.Value.Date);

            ViewBag.HasSearched = true;

            try
            {
                return View(await rides.ToListAsync());
            }
            catch (Exception)
            {
                ViewBag.SearchError = "Ride data could not be loaded because the local database is currently unavailable.";
                return View(new List<Ride>());
            }
        }

        // GET: Rides/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ride = await _context.Rides
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ride == null)
            {
                return NotFound();
            }

            return View(ride);
        }

        // GET: Rides/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rides/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FromLocation,ToLocation,DepartureTime,TotalSeats,PricePerSeat")] Ride ride)
        {
            
            if (ModelState.IsValid)
            {
                ride.RideId = await GenerateUniqueRideId();
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized();
                }

                if (user.DriverNumber == 0)
                {
                    user.DriverNumber = new Random().Next(1000, 9999);
                    await _userManager.UpdateAsync(user);
                }

                ride.DriverId = user.DriverNumber.ToString();
                ride.AvailableSeats = ride.TotalSeats;
                ride.CreatedAt = DateTime.Now;
                _context.Add(ride);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ride);
        }

        // GET: Rides/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ride = await _context.Rides.FindAsync(id);
            if (ride == null)
            {
                return NotFound();
            }
            return View(ride);
        }

        // POST: Rides/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FromLocation,ToLocation,DepartureTime,TotalSeats,PricePerSeat")] Ride ride)
        {
            if (id != ride.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRide = await _context.Rides.FindAsync(id);

                    if (existingRide == null)
                        return NotFound();

                    // Update only allowed fields
                    existingRide.FromLocation = ride.FromLocation;
                    existingRide.ToLocation = ride.ToLocation;
                    existingRide.DepartureTime = ride.DepartureTime;
                    existingRide.TotalSeats = ride.TotalSeats;
                    existingRide.PricePerSeat = ride.PricePerSeat;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RideExists(ride.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(ride);
        }

        // GET: Rides/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ride = await _context.Rides
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ride == null)
            {
                return NotFound();
            }

            return View(ride);
        }

        // POST: Rides/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ride = await _context.Rides.FindAsync(id);
            if (ride != null)
            {
                _context.Rides.Remove(ride);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RideExists(int id)
        {
            return _context.Rides.Any(e => e.Id == id);
        }
        private static readonly Random _random = new Random();

        private async Task<string> GenerateUniqueRideId()
        {
            while (true)
            {
                var id = _random.Next(100000, 999999).ToString();

                var exists = await _context.Rides.AnyAsync(r => r.RideId == id);

                if (!exists)
                    return id;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int rideId)
        {
            var ride = await _context.Rides.FindAsync(rideId);

            if (ride == null)
                return NotFound();

            // Seats check (MISSING RIGHT NOW)
            if (ride.AvailableSeats <= 0)
            {
                TempData["Error"] = "Ride is fully booked";
                return RedirectToAction("FindRide");
            }

            if (ride.DepartureTime < DateTime.Now)
            {
                TempData["Error"] = "This ride has already departed";
                return RedirectToAction("FindRide");
            }

            var vm = new CheckoutViewModel
            {
                RideId = ride.Id,
                FromLocation = ride.FromLocation,
                ToLocation = ride.ToLocation,
                DepartureTime = ride.DepartureTime,
                PricePerSeat = ride.PricePerSeat
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            var ride = await _context.Rides.FindAsync(vm.RideId);

            var userId = User.Identity?.Name;

            // prevent duplicate booking
            var alreadyBooked = await _context.Bookings
                .AnyAsync(b => b.RideId == vm.RideId && b.PassengerId == userId);

            if (alreadyBooked)
            {
                TempData["Error"] = "You have already booked this ride";
                return RedirectToAction("FindRide");
            }

            if (ride == null)
                return NotFound();

            // Seats check (MISSING RIGHT NOW)
            if (ride.AvailableSeats <= 0)
            {
                TempData["Error"] = "Ride is fully booked";
                return RedirectToAction("FindRide");
            }


            if (ride.DepartureTime < DateTime.Now)
            {
                TempData["Error"] = "This ride has already departed";
                return RedirectToAction("FindRide");
            }

            if (string.IsNullOrEmpty(vm.PaymentMethod))
            {
                ModelState.AddModelError("", "Select payment method");
                return View(vm);
            }

            ride.AvailableSeats -= 1;

            var user = await _userManager.GetUserAsync(User);

            var booking = new Booking
            {
                RideId = ride.Id,
                PassengerId = user?.Id,
                BookingTime = DateTime.Now,
                PaymentMethod = vm.PaymentMethod
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // EMAIL STARTS HERE

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Ride Booking Confirmed",
                    $"Your ride from {ride.FromLocation} to {ride.ToLocation} on {ride.DepartureTime} has been successfully booked."
                );
            }
            // EMAIL ENDS HERE
            return RedirectToAction("Confirmation", new { id = booking.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.Identity?.Name;

            var bookings = await _context.Bookings
                .Include(b => b.Ride)
                .Where(b => b.PassengerId == userId)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();

            return View(bookings);
        }

       
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Ride)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // restore seat
            if (booking.Ride != null)
            {
                booking.Ride.AvailableSeats += 1;
            }

            // remove booking
            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction("MyBookings");
        }
    }
}
