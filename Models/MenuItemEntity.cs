using System;
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    public class MenuItemEntity : ITableEntity
    {
        // PartitionKey: Category string (e.g., "Hot Drinks", "Cold Drinks", "Pastries", "Sandwiches")
        public string PartitionKey { get; set; } = string.Empty;

        // RowKey: Unique Item SKU / ID (e.g., "COF-001", "PAS-104")
        public string RowKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Mandatory ITableEntity properties
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class CreateMenuItemDto
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateMenuItemDto
    {
        public double? Price { get; set; }
        public bool? IsAvailable { get; set; }
    }
}