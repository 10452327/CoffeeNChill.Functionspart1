using System.Net;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class MenuFunctions
    {
        private readonly ILogger _logger;
        private readonly TableClient _tableClient;

        public MenuFunctions(ILoggerFactory loggerFactory, TableServiceClient tableServiceClient)
        {
            _logger = loggerFactory.CreateLogger<MenuFunctions>();
            _tableClient = tableServiceClient.GetTableClient("MenuItems");
            _tableClient.CreateIfNotExists();
        }

        // 1. POST /api/menu - Inserts new entity into MenuItems
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> CreateMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Inserting menu item into MenuItems table.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<CreateMenuItemDto>(requestBody, options);

            if (dto == null || string.IsNullOrWhiteSpace(dto.PartitionKey) || string.IsNullOrWhiteSpace(dto.RowKey))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("PartitionKey (Category) and RowKey (SKU) are required.");
                return bad;
            }

            var entity = new MenuItemEntity
            {
                PartitionKey = dto.PartitionKey,
                RowKey = dto.RowKey,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable
            };

            await _tableClient.UpsertEntityAsync(entity);

            var res = req.CreateResponse(HttpStatusCode.Created);
            await res.WriteAsJsonAsync(entity);
            return res;
        }

        // 2. GET /api/menu - Queries and returns all menu entities
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> GetAllMenuItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Querying all menu items.");

            var items = new List<MenuItemEntity>();
            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>())
            {
                items.Add(item);
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(items);
            return res;
        }

        // 3. GET /api/menu/category/{category} - Filters entities by PartitionKey
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> GetMenuItemsByCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            _logger.LogInformation($"Filtering menu items by category: {category}");

            var items = new List<MenuItemEntity>();
            string filter = TableClient.CreateQueryFilter<MenuItemEntity>(e => e.PartitionKey == category);

            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>(filter))
            {
                items.Add(item);
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(items);
            return res;
        }

        // 4. PUT /api/menu/{category}/{id} - Updates price or availability
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> UpdateMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category,
            string id)
        {
            _logger.LogInformation($"Updating menu item {category}/{id}");

            try
            {
                var existing = await _tableClient.GetEntityAsync<MenuItemEntity>(category, id);
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dto = JsonSerializer.Deserialize<UpdateMenuItemDto>(requestBody, options);

                if (dto != null)
                {
                    if (dto.Price.HasValue) existing.Value.Price = dto.Price.Value;
                    if (dto.IsAvailable.HasValue) existing.Value.IsAvailable = dto.IsAvailable.Value;
                    await _tableClient.UpdateEntityAsync(existing.Value, existing.Value.ETag, TableUpdateMode.Replace);
                }

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(existing.Value);
                return res;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Item {category}/{id} not found.");
                return notFound;
            }
        }

        // 5. DELETE /api/menu/{category}/{id} - Removes item
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> DeleteMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category,
            string id)
        {
            _logger.LogInformation($"Deleting menu item {category}/{id}");

            try
            {
                await _tableClient.DeleteEntityAsync(category, id);
                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteStringAsync($"Item {category}/{id} successfully deleted.");
                return res;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Item {category}/{id} not found.");
                return notFound;
            }
        }
    }
}