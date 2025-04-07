using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Data;
using PharmacyQueue.Models;

namespace PharmacyQueue.Controllers
{
    public class AdminController : Controller
    {
        private readonly PharmacyDbContext db;

        public AdminController(PharmacyDbContext context)
        {
            db = context;
        }

        // Shows the login page, but only if the user isn't already logged in
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        // Handles the login form submission - checks if the admin exists and lets them in if credentials are correct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Check if email and password are provided
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password required.";
                return View();
            }

            // Look up admin in database and check if they're active
            var admin = await db.Admins.FirstOrDefaultAsync(a => a.Email == email && a.Password == password && a.IsActive);
            if (admin == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            // Create a claims identity for the logged-in admin
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, admin.Name)
            }, "login");

            // Sign in the admin and update their last login time
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            admin.LastLogin = DateTime.Now;
            await db.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // Shows the main dashboard with all appointments, sorted by newest first
        public async Task<IActionResult> Dashboard()
        {
            // Get all appointments ordered by creation time (newest first)
            var data = await db.Appointments.OrderByDescending(x => x.CreatedTime).ToListAsync();

            // Get today's date in Toronto timezone for proper date handling
            var torontoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, torontoTimeZone);

            // Pass today's date to the view in both regular and ISO format
            ViewBag.Today = today;
            ViewBag.TodayISO = today.ToString("yyyy-MM-dd");

            return View(data);
        }

        // Updates how far along an appointment is (like waiting, in progress, done) and adds any notes
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, int status, string? notes)
        {
            // Find the appointment and update its status and timestamp
            var app = await db.Appointments.FindAsync(id);
            if (app != null)
            {
                app.Status = status;
                app.LastUpdated = DateTime.Now;
                // Only update notes if they're provided
                if (!string.IsNullOrEmpty(notes))
                {
                    app.AdditionalNotes = notes;
                }
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }

        // Adds or updates remarks for completed appointments
        [HttpPost]
        public async Task<IActionResult> UpdateRemarks(int id, string remarks)
        {
            // Only allow remarks for completed appointments (status 2)
            var a = await db.Appointments.FindAsync(id);
            if (a != null && a.Status == 2)
            {
                // a.Remarks = remarks;
                // await db.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }

        // Soft deletes an appointment by marking it as cancelled (only works for waiting or completed appointments)
        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            // Find the appointment and only allow deletion if it's waiting (0) or completed (2)
            var a = await db.Appointments.FindAsync(id);
            if (a != null && (a.Status == 0 || a.Status == 2))
            {
                a.Status = 4; // Status 4 represents cancelled appointments
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }

        // Logs the admin out and sends them back to the home page
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Shows a calendar view of all appointments, grouped by date
        public async Task<IActionResult> Calendar()
        {
            // Get all appointments ordered by creation time
            var appointments = await db.Appointments
                .OrderBy(a => a.CreatedTime)
                .ToListAsync();

            // Group appointments by their date for easier display
            var appointmentsByDate = appointments
                .GroupBy(a => a.CreatedTime.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.AppointmentsByDate = appointmentsByDate;
            return View();
        }

        // API endpoint that gets all appointments, optionally filtered by date, for the calendar view
        [HttpGet]
        [Route("/api/admin/appointments")]
        public async Task<IActionResult> GetAppointments([FromQuery] string? date = null)
        {
            var query = db.Appointments.AsQueryable();

            // Filter appointments by specific date if provided
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var selectedDate))
            {
                query = query.Where(a => a.CreatedTime.Date == selectedDate.Date);
            }

            // Get all matching appointments ordered by creation time
            var appointments = await query
                .OrderByDescending(a => a.CreatedTime)
                .ToListAsync();

            // Convert appointments to calendar events with color coding based on status
            var events = appointments.Select(a => new
            {
                id = a.AppointmentID,
                title = $"{a.Name} - {a.Purpose}",
                start = a.CreatedTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                allDay = false,
                // Color coding for different appointment statuses
                backgroundColor = a.Status switch
                {
                    0 => "#ffc107", // Yellow for Waiting
                    1 => "#0dcaf0", // Blue for In Progress
                    2 => "#198754", // Green for Completed
                    4 => "#dc3545", // Red for Cancelled
                    _ => "#6c757d"  // Gray for Unknown
                },
                borderColor = "transparent",
                textColor = "#000000",
                // Additional properties for the calendar event
                extendedProps = new
                {
                    id = a.AppointmentID,
                    name = a.Name,
                    email = a.Email,
                    phone = a.Phone,
                    purpose = a.Purpose,
                    status = a.Status,
                    // Human-readable status text
                    statusText = a.Status switch
                    {
                        0 => "Waiting",
                        1 => "In Progress",
                        2 => "Completed",
                        4 => "Cancelled",
                        _ => "Unknown"
                    },
                    queueNumber = a.QueueNumber,
                    additionalNotes = a.AdditionalNotes,
                    date = a.CreatedTime.ToString("yyyy-MM-dd")
                }
            }).ToList();

            return Json(events);
        }

        // Adds or updates notes for any appointment
        [HttpPost]
        public async Task<IActionResult> AddNotes(int id, string notes)
        {
            // Find the appointment and update its notes and timestamp
            var app = await db.Appointments.FindAsync(id);
            if (app != null)
            {
                app.AdditionalNotes = notes;
                app.LastUpdated = DateTime.Now;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }
    }
}
