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
    public static class DeploymentRequestProcess
    {
        [FunctionName("DeploymentRequestProcess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string sessionId = null;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            sessionId = sessionId ?? data?.sessionId;

            if (string.IsNullOrEmpty(sessionId))
            {
                log.LogInformation("Messgae not sent to service bus.");
            }
            else 
            {
                // Send message to ServiceBus
                await using (ServiceBusClient client = new ServiceBusClient(System.Environment.GetEnvironmentVariable("ServiceBusConnection")))
                {
                    string queueName = "wvd-request-session";
                    ServiceBusSender sender = client.CreateSender(queueName);
                    ServiceBusMessage message = new ServiceBusMessage(requestBody);
                    message.SessionId = sessionId;
                    await sender.SendMessageAsync(message);
                }
                log.LogInformation("Message sent to service bus.");
            }
            return new OkObjectResult(requestBody);
        }
    }
}

