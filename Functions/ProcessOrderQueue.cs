using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions
{
    public static class ProcessOrderQueue
    {
        [FunctionName("ProcessOrderQueue")]
        public static async Task Run(
            [QueueTrigger("order-processing-queue", Connection = "AzureWebJobsStorage")] string myQueueItem,
            ILogger log)
        {
            if (string.IsNullOrWhiteSpace(myQueueItem)) return;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            OrderDto order = JsonSerializer.Deserialize<OrderDto>(myQueueItem, options);

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true;";
            TableServiceClient tableServiceClient = new TableServiceClient(connectionString);
            TableClient tableClient = tableServiceClient.GetTableClient("Orders");
            await tableClient.CreateIfNotExistsAsync();

            string partitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string rowKey = order.OrderId;

            // Initial State: Received
            var orderEntity = new TableEntity(partitionKey, rowKey)
            {
                { "CustomerName", order.CustomerName },
                { "TotalPrice", (double)order.TotalPrice },
                { "Status", "Received" },
                { "OrderTimestamp", order.OrderTimestamp ?? DateTime.UtcNow.ToString("o") }
            };
            await tableClient.UpsertEntityAsync(orderEntity);

            // Transition to Preparing
            await Task.Delay(2000);
            orderEntity["Status"] = "Preparing";
            await tableClient.UpdateEntityAsync(orderEntity, orderEntity.ETag, TableUpdateMode.Replace);

            // Transition to Ready
            await Task.Delay(2000);
            orderEntity["Status"] = "Ready";
            await tableClient.UpdateEntityAsync(orderEntity, orderEntity.ETag, TableUpdateMode.Replace);

            // Transition to Collected
            await Task.Delay(2000);
            orderEntity["Status"] = "Collected";
            await tableClient.UpdateEntityAsync(orderEntity, orderEntity.ETag, TableUpdateMode.Replace);

            log.LogInformation($"Order {order.OrderId} completed full lifecycle successfully.");
        }
    }
}