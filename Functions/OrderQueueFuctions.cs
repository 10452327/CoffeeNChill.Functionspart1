using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions
{
    public static class OrderQueueFunctions
    {
        [Function("QueueOrderProducer")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("QueueOrderProducer");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Request body cannot be empty." });
                return badResponse;
            }

            OrderDto order;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                order = JsonSerializer.Deserialize<OrderDto>(requestBody, options);
            }
            catch (JsonException)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid JSON format." });
                return badResponse;
            }

            if (order == null || string.IsNullOrWhiteSpace(order.OrderId) || string.IsNullOrWhiteSpace(order.CustomerName) || order.SelectedItemSKUs == null || order.SelectedItemSKUs.Count == 0)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Missing required fields (OrderId, CustomerName, SelectedItemSKUs)." });
                return badResponse;
            }

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true;";
            QueueClient queueClient = new QueueClient(connectionString, "order-processing-queue");
            await queueClient.CreateIfNotExistsAsync();

            string messagePayload = JsonSerializer.Serialize(order);
            string base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messagePayload));
            await queueClient.SendMessageAsync(base64Payload);

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(new { status = "Success", message = "Order successfully queued.", orderId = order.OrderId });
            return okResponse;
        }
    }
}