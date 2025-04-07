using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Data;
using PharmacyQueue.Models;
using System.Net.Mail;

namespace PharmacyQueue.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly PharmacyDbContext _context;
        private readonly IConfiguration _config;

        public AppointmentController(PharmacyDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Show booking form
        public IActionResult Book()
        {
            return View(new Appointment());
        }

        // Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Appointment appointment)
        {
            if (string.IsNullOrEmpty(appointment.Name) ||
                string.IsNullOrEmpty(appointment.Email) ||
                string.IsNullOrEmpty(appointment.Phone) ||
                string.IsNullOrEmpty(appointment.Purpose))
            {
                ModelState.AddModelError("", "Please fill all fields.");
                return View(appointment);
            }

            var today = DateTime.Today;
            var totalToday = await _context.Appointments.CountAsync(a => a.CreatedTime.Date == today);
            var settings = await _context.QueueSettings.FirstOrDefaultAsync();

            if (settings != null && totalToday >= settings.MaxDailyBookings)
            {
                ModelState.AddModelError("", "Daily booking limit reached. Try again tomorrow.");
                return View(appointment);
            }

            var queueNumber = $"PHAR-{today:yyyyMMdd}-{(totalToday + 1):D3}";

            var newAppointment = new Appointment
            {
                Name = appointment.Name,
                Email = appointment.Email,
                Phone = appointment.Phone,
                Purpose = appointment.Purpose == "Other" && !string.IsNullOrEmpty(appointment.AdditionalNotes)
                    ? $"Other - {appointment.AdditionalNotes}" : appointment.Purpose,
                AdditionalNotes = appointment.AdditionalNotes,
                QueueNumber = queueNumber,
                Status = 0,
                CreatedTime = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                AppointmentID = newAppointment.AppointmentID,
                Type = "Confirmation",
                EmailSent = false,
                EmailContent = GenerateEmailBody(newAppointment),
                NotificationTime = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendEmail(newAppointment, notification);

            return RedirectToAction("Confirmation", new { queueNumber = newAppointment.QueueNumber });
        }

        public async Task<IActionResult> Confirmation(string queueNumber)
        {
            if (string.IsNullOrEmpty(queueNumber))
                return NotFound();

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.QueueNumber == queueNumber);

            if (appointment == null)
                return NotFound();

            var aheadCount = await _context.Appointments.CountAsync(a =>
                a.CreatedTime.Date == appointment.CreatedTime.Date &&
                a.Status == 0 &&
                a.CreatedTime < appointment.CreatedTime);

            var settings = await _context.QueueSettings.FirstOrDefaultAsync();
            ViewBag.Position = aheadCount + 1;
            ViewBag.EstimatedWaitTime = (aheadCount + 1) * (settings?.AverageWaitTime ?? 15);

            return View(appointment);
        }

        public async Task<IActionResult> Status(string? queueNumber)
        {
            if (string.IsNullOrEmpty(queueNumber))
                return View(null);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.QueueNumber == queueNumber);

            if (appointment == null)
            {
                ViewBag.Error = "Queue number not found.";
                return View(null);
            }

            return View(appointment);
        }

        // Email message content
        private string GenerateEmailBody(Appointment a)
        {
            var notes = !string.IsNullOrEmpty(a.AdditionalNotes) ? $"\nNotes: {a.AdditionalNotes}" : "";

            return $@"Hi {a.Name},

Your appointment has been booked.

Queue Number: {a.QueueNumber}

Details:
- Purpose: {a.Purpose}{notes}
- Date: {a.CreatedTime:MMM dd, yyyy}
- Time: {a.CreatedTime:hh:mm tt}

You can check your queue status anytime on our website.

Thanks,
Pharmacy Team";
        }

        // Send confirmation email
        private async Task SendEmail(Appointment a, Notification n)
        {
            try
            {
                var settings = _config.GetSection("EmailSettings");
                var smtp = new SmtpClient(settings["SmtpHost"])
                {
                    Port = int.Parse(settings["SmtpPort"] ?? "587"),
                    Credentials = new System.Net.NetworkCredential(settings["SenderEmail"], settings["SenderPassword"]),
                    EnableSsl = true
                };

                var msg = new MailMessage
                {
                    From = new MailAddress(settings["SenderEmail"], settings["SenderName"]),
                    Subject = $"Your Queue Number: {a.QueueNumber}",
                    Body = n.EmailContent,
                    IsBodyHtml = false
                };

                msg.To.Add(a.Email);
                await smtp.SendMailAsync(msg);

                n.EmailSent = true;
                await _context.SaveChangesAsync();
            }
            catch
            {
                // quietly fail email sending if not critical
            }
        }
    }
}
