using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace MK.WVD
{
    public static class SubmissionProcess
    {
        public class WvdRequest
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string sessionId { get; set; }
            public string name { get; set; }
            public string wvdPool { get; set; }
            public int number { get; set; }
        }

        [FunctionName("SubmissionProcess")]
        [return: Table("wvdconfig", Connection = "TableStorageConnection")]
        public static WvdRequest Run([ServiceBusTrigger("wvd-request-session", Connection = "ServiceBusConnection", IsSessionsEnabled = true)]
        string myQueueItem,
        string MessageId,
         ILogger log)
        {
            dynamic data = JsonConvert.DeserializeObject(myQueueItem);
            WvdRequest myWvdRequest = new WvdRequest{
                PartitionKey = data?.name,
                RowKey = Guid.NewGuid().ToString(),
                name = data?.name,
                sessionId = data?.sessionId,
                wvdPool = data?.wvdPool,
                number = data?.number
            };

            // Send message to ServiceBus
            ServiceBusClient client = new ServiceBusClient(System.Environment.GetEnvironmentVariable("ServiceBusConnection"));
            string queueName = "wvd-response-session";
            ServiceBusSender sender = client.CreateSender(queueName);
            ServiceBusMessage message = new ServiceBusMessage(JsonConvert.SerializeObject(myWvdRequest));
            message.SessionId = myWvdRequest.sessionId;
            sender.SendMessageAsync(message);
    
            log.LogInformation("Message sent to service bus.");

            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            return myWvdRequest;
        }
    }
}