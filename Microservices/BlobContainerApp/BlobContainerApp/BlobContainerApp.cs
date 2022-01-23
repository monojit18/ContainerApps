using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BlobContainerApp
{
    public static class BlobContainerApp
    {
        [FunctionName("BlobContainerApp")]
        public static async Task Run([BlobTrigger("contappblob/{name}", Connection = "AzureWebJobsStorage")]
                                        Stream myBlob, string name,
                                        [Queue("contappqueue",
                                        Connection = "AzureWebJobsStorage")]
                                        IAsyncCollector<CloudQueueMessage> cloudQueueMessageCollector,
                                        ILogger log)
        {
            
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            var cloudQueueMessage = new CloudQueueMessage(name);
            await cloudQueueMessageCollector.AddAsync(cloudQueueMessage);

        }
    }
}
