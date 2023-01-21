using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace PollyTest.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : Controller
    {
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        readonly AsyncFallbackPolicy<HttpResponseMessage> _httpFallbackPolicy;
        private int fallBackNumber = 21;

        public CatalogController()
        {
            int maxRetryCount = 3;
            int retryCount = 0;
            //Immediate retry policy
            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(maxRetryCount, onRetry: (response, timeSpan) =>
                {
                    Console.WriteLine(retryCount++);
                });

            ////Exponential wait and retry policy
            //_httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //    .WaitAndRetryAsync(maxRetryCount, sleepDurationProvider: retryAttempt =>
            //    {
            //        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2);
            //    });

            ////Exponential wait and retry policy with a fallback policy
            _httpFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(fallBackNumber.GetType(),fallBackNumber, new JsonMediaTypeFormatter())
                });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = new HttpClient();
            string requestEndpoint = "https://" + Request.Host.Value + $"/inventory/{id}";

            //HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);
            //HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            HttpResponseMessage response = await _httpFallbackPolicy.ExecuteAsync(
                ()=> _httpRetryPolicy.ExecuteAsync(
                    () => httpClient.GetAsync(requestEndpoint)));

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
