using Microsoft.Azure.Functions.Worker.Extensions.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class ProcessOrderQueue
    {
        private readonly ILogger<ProcessOrderQueue> _logger;

        public ProcessOrderQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessOrderQueue>();
        }

        [Function(nameof(ProcessOrderQueue))]
        [TableOutput("Orders", Connection = "AzureWebJobsStorage")]
        public OrderEntity Run(
            [QueueTrigger("order-processing-queue", Connection = "AzureWebJobsStorage")] OrderPayload message)
        {
            _logger.LogInformation($"Processing order: {message.OrderId} for {message.CustomerName}");

            // Map incoming queue payload to Table Storage Entity
            return new OrderEntity
            {
                PartitionKey = "CoffeeOrders",
                RowKey = message.OrderId,
                CustomerName = message.CustomerName,
                TotalPrice = message.TotalPrice,
                Status = "Processed"
            };
        }
    }

    // Helper classes matching your payload & table schema
    public class OrderPayload
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}