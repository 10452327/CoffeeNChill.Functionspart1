using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions
{
    public class DocumentFunctions
    {
        private readonly ILogger _logger;
        private readonly BlobContainerClient _containerClient;

        public DocumentFunctions(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
        {
            _logger = loggerFactory.CreateLogger<DocumentFunctions>();
            _containerClient = blobServiceClient.GetBlobContainerClient("staff-docs");
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        // 1. POST /api/documents/upload - Accepts multipart/form-data and streams to staff-docs
        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> UploadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            _logger.LogInformation("Uploading file to staff-docs blob container.");

            try
            {
                var parsedForm = await MultipartFormDataParser.ParseAsync(req.Body);
                var file = parsedForm.Files.FirstOrDefault();

                if (file == null)
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("No file provided in the request.");
                    return bad;
                }

                string fileName = Path.GetFileName(file.FileName);
                var blobClient = _containerClient.GetBlobClient(fileName);

                using (var stream = file.Data)
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully to staff-docs",
                    fileName = fileName,
                    size = file.Data.Length
                });
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Upload failed: {ex.Message}");
                return err;
            }
        }

        // 2. GET /api/documents - Lists all stored files in staff-docs
        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> ListStaffDocuments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            _logger.LogInformation("Listing files in staff-docs.");

            var documents = new List<object>();
            await foreach (BlobItem blob in _containerClient.GetBlobsAsync())
            {
                documents.Add(new
                {
                    name = blob.Name,
                    size = blob.Properties.ContentLength ?? 0,
                    lastModified = blob.Properties.LastModified?.ToString("o") ?? string.Empty,
                    contentType = blob.Properties.ContentType ?? "application/octet-stream"
                });
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(documents);
            return res;
        }

        // 3. GET /api/documents/download/{fileName} - Streams recipe/manual back
        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation($"Downloading file: {fileName}");

            var blobClient = _containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync($"Document '{fileName}' not found.");
                return notFound;
            }

            var downloadInfo = await blobClient.DownloadAsync();

            var res = req.CreateResponse(HttpStatusCode.OK);
            res.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            res.Headers.Add("Content-Type", downloadInfo.Value.ContentType ?? "application/octet-stream");

            await downloadInfo.Value.Content.CopyToAsync(res.Body);
            return res;
        }
    }
}