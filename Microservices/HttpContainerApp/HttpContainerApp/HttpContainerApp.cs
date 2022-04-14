using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpContainerApps
{
    public static class HttpContainerApps
    {
        [FunctionName("container")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            log.LogInformation("C# HTTP trigger function processed a request.");

            var name = req.Query["name"];

            var uri = $"{Environment.GetEnvironmentVariable("HTTP_BACKEND_URL")}?name={name}";
            var res = await cl.GetAsync(uri);
            var response = await res.Content.ReadAsStringAsync();
            log.LogInformation($"Status:{res.StatusCode}");
            log.LogInformation($"Response:{response}-v1.0.4");
            response = $"Hello, {response}-v1.0.4";
            // var response = $"Secured, {name}-v1.0.3";
            // var response = $"Basic, {name}-v1.0.2";
            var response = $"Basic, {name}-v1.0.1";
            return new OkObjectResult(response);

        }
    }
}
