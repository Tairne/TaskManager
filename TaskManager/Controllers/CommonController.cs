using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Route("api")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IApplicationMetrics _metrics;

        public CommonController(IApplicationMetrics metrics)
        {
            _metrics = metrics;
        }

        [Authorize]
        [HttpGet("metrics")]
        public ActionResult<ApplicationMetricsSnapshot> GetMetrics()
        {
            return Ok(_metrics.GetSnapshot());
        }
    }
}
