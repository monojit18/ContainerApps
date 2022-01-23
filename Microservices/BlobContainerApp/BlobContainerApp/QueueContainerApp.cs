using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BlobContainerApp
{
    public static class QueueContainerApp
    {
        [FunctionName("queuecontainerapp")]
        public static void Run([QueueTrigger("contappqueue", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
