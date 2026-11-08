using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using EcommerceServerless.App.Models;

namespace EcommerceServerless.App
{
    public static class AddOrder
    {
        [FunctionName("AddOrder")]
        [return: ServiceBus("order-created", Connection = "ServiceBusCs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Order data = JsonConvert.DeserializeObject<Order>(requestBody);

            log.LogInformation($"Order {data.id} created.");

            return new OkObjectResult(data);
        }
    }
}
