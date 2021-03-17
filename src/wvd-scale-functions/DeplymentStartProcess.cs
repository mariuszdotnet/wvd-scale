using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace MK.WVD
{
    public static class DeplymentStartProcess
    {
        [FunctionName("DeplymentStartProcess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string sessionId = null;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            sessionId = sessionId ?? data?.sessionId;
            string responseMessage = null;

            if (string.IsNullOrEmpty(sessionId))
            {
                log.LogInformation("Message not sent to service bus.");
            }
            else 
            {
                // Send message to ServiceBus
                await using (ServiceBusClient client = new ServiceBusClient(System.Environment.GetEnvironmentVariable("ServiceBusConnection")))
                {
                    ServiceBusSessionReceiver receiver = await client.AcceptSessionAsync("wvd-response-session", sessionId);
                    ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();


                    log.LogInformation(receivedMessage.MessageId);
                    log.LogInformation(receivedMessage.Body.ToString());

                    await receiver.CompleteMessageAsync(receivedMessage);
                    responseMessage = receivedMessage.Body.ToString();
                }
                log.LogInformation("Message read from service bus.");
            }


            return new OkObjectResult(responseMessage);
        }
    }
}

