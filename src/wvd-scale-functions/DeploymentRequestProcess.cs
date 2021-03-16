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
        //static string connectionString = "";
        static string queueName = "wvd-request-session";

        [FunctionName("DeploymentRequestProcess")]
        //[return: ServiceBus("myqueue", Connection = "ServiceBusConnection")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            await using (ServiceBusClient client = new ServiceBusClient(System.Environment.GetEnvironmentVariable("ServiceBusConnection")))
            {
                // create a sender for the queue
                ServiceBusSender sender = client.CreateSender(queueName);

                // create a message that we can send
                ServiceBusMessage message = new ServiceBusMessage("Hello world!");
                //message.MessageId = "supermario";
                message.SessionId = "1";

                // send the message
                await sender.SendMessageAsync(message);
            }

            return new OkObjectResult(responseMessage);
        }
    }
}

