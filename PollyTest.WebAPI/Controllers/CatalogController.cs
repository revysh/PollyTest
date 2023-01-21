using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PollyTest.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = new HttpClient();
            string requestEndpoint = "https://" + Request.Host.Value + $"/inventory/{id}";

            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);

            if(response.IsSuccessStatusCode) {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
