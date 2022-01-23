using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpContainerApp
{
    public static class HttpLogicContainerApp
    {

        private static HttpClient httpClient = new HttpClient();

        [FunctionName("logicapp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestMessage requestMessage, ILogger log)
        {

            var body = await requestMessage.Content.ReadAsStringAsync();
            var logicAppCallbackUri = Environment.GetEnvironmentVariable("LOGICAPP_CALLBACK_URL");
            var logicAppPostUri = Environment.GetEnvironmentVariable("LOGICAPP_POST_URL");

            var responseMessage = await httpClient.PostAsync(logicAppCallbackUri, null);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var callbackModel = JsonConvert.DeserializeObject<CallbackModel>(responseContent);

            var signature = callbackModel.Queries.Signature;
            logicAppPostUri = string.Format(logicAppPostUri, signature);

            responseMessage = await httpClient.PostAsync(logicAppPostUri, new StringContent(body));
            responseContent = await responseMessage.Content.ReadAsStringAsync();

            var zipModel = JsonConvert.DeserializeObject<ZipModel>(responseContent);
            return new OkObjectResult(zipModel);

        }
    }
}
