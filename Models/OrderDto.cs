using System.Collections.Generic;

namespace CoffeeNChill.Functions.Models
{
    public class OrderDto
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public List<string> SelectedItemSKUs { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderTimestamp { get; set; }
    }
}