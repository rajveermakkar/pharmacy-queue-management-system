using Microsoft.AspNetCore.Mvc;
using PharmacyQueue.Models;
using PharmacyQueue.Data;
using System.Diagnostics;
using System.Linq;

namespace PharmacyQueue.Controllers
{
    public class HomeController : Controller
    {
        private PharmacyDbContext db;

        public HomeController(PharmacyDbContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;

            // Count how many people are still waiting in the queue
            var waitingPeople = db.Appointments
                .Where(x => x.CreatedTime.Date == today && x.Status == 0)
                .Count();

            // Get average wait time from settings table
            var settings = db.QueueSettings.FirstOrDefault();
            int avgWait = 15; // default
            if (settings != null)
            {
                avgWait = settings.AverageWaitTime;
            }

            // Send values to view
            ViewBag.PeopleInQueue = waitingPeople;
            ViewBag.AverageWaitTime = avgWait;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
