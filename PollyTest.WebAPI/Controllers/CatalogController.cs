using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Retry;

namespace PollyTest.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : Controller
    {
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public CatalogController()
        {
            int retryCount = 0;
            //Immediate retry policy
            //_httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //    .RetryAsync(3, onRetry: (response, timeSpan) => {
            //        Console.WriteLine(retryCount++);
            //    });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = new HttpClient();
            string requestEndpoint = "https://" + Request.Host.Value + $"/inventory/{id}";

            //HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);
            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
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
