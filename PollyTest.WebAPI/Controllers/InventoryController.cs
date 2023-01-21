using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace PollyTest.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : Controller
    {
        static int _requestCount = 0;
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100);
            _requestCount++;
            if(_requestCount%4 == 0)
            {
                return Ok(15);
            }
            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong.");
        }
    }
}
