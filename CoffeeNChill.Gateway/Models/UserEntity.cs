
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Gateway.Models
{
    public class UserEntity : ITableEntity
    {
        // PartitionKey: Admin, Staff, or Customer
        public string PartitionKey { get; set; } = string.Empty;

        // RowKey: User Email / Username
        public string RowKey { get; set; } = string.Empty;

        // Stores the hashed password, never the plain-text password
        public string PasswordHash { get; set; } = string.Empty;

        // User's full name
        public string FullName { get; set; } = string.Empty;

        // Date the user account was created
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Additional aliases, if required by your rubric
        public string PartyKey => PartitionKey;
        public string Id => RowKey;

        // Required by ITableEntity
        public ETag ETag { get; set; } = ETag.All;

        public DateTimeOffset? Timestamp { get; set; }
    }
}
