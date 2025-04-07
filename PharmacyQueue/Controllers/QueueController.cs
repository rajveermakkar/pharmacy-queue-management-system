using Microsoft.AspNetCore.Mvc;
using PharmacyQueue.Services;

namespace PharmacyQueue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private QueueService queueService;

        public QueueController(QueueService service)
        {
            queueService = service;
        }

        [HttpGet("status/{queueNumber}")]
        public IActionResult GetQueueStatus(string queueNumber)
        {
            var status = queueService.GetQueueStatus(queueNumber).Result;

            if (!status.IsValid)
            {
                return NotFound("Queue not found");
            }

            return Ok(status);
        }
    }
}
