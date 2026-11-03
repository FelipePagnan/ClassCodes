using EcommerceServerless.App.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace EcommerceServerless.App
{
    public static class PaymentApproved
    {
        [FunctionName("PaymentApproved")]
        public static void Run([CosmosDBTrigger(
            databaseName: "ecommerce-serverless",
            containerName: "order-receipts",
            Connection  = "CosmosDbCs",
            LeaseContainerName  = "leases")]IReadOnlyList<PaymentReceipt> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var item in input)
                {
                    log.LogInformation($"Payment receipt {item.id}, paid at {item.paidAt}");
                }
            }
        }
    }
}
