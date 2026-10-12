using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions
{
    public static class OrderQueueFunctions
    {
        [FunctionName("QueueOrderProducer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body cannot be empty." });
            }

            OrderDto order;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                order = JsonSerializer.Deserialize<OrderDto>(requestBody, options);
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult(new { error = "Invalid JSON format." });
            }

            if (order == null || string.IsNullOrWhiteSpace(order.OrderId) || string.IsNullOrWhiteSpace(order.CustomerName) || order.SelectedItemSKUs == null || order.SelectedItemSKUs.Count == 0)
            {
                return new BadRequestObjectResult(new { error = "Missing required fields (OrderId, CustomerName, SelectedItemSKUs)." });
            }

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true;";
            QueueClient queueClient = new QueueClient(connectionString, "order-processing-queue");
            await queueClient.CreateIfNotExistsAsync();

            string messagePayload = JsonSerializer.Serialize(order);
            string base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messagePayload));
            await queueClient.SendMessageAsync(base64Payload);

            return new OkObjectResult(new { status = "Success", message = "Order successfully queued.", orderId = order.OrderId });
        }
    }
}