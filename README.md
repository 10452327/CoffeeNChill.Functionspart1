CoffeeNChill – Cloud Development (Part 2)
Project Overview
CoffeeNChill is a cloud-based coffee shop management system developed as part of the CLDV6212 Cloud Development module (IIE, 2026a). The purpose of the project is to demonstrate the development of a cloud-ready application that provides RESTful API functionality for managing coffee shop menu items, staff documents, and an asynchronous order-processing queue workflow (IIE, 2026a; IIE, 2026b). The application is developed using C# with .NET 10 and Azure Functions v4 using the .NET isolated worker model (IIE, 2026a). Azure Storage services are used to provide persistent storage for the application's data and handle peak-hour order queues, while Azurite is used as a local emulator during development and testing (IIE, 2026a; IIE, 2026b). The project also demonstrates multi-container orchestration using Docker Compose and Docker Hub, API testing using Postman, and version control using Git and GitHub (IIE, 2026a; IIE, 2026b).   
JPEG
+ 4
Technologies Used
The main technologies used in the development of CoffeeNChill are C#, .NET 10, Azure Functions v4, Azure Table Storage, Azure File Share, Azure Storage Queues, Azurite, Docker, Docker Hub, Postman, Visual Studio and GitHub (IIE, 2026a; IIE, 2026b). C# is used as the programming language for implementing the application's models, services and Azure Functions (IIE, 2026a). .NET 10 provides the development framework, while Azure Functions provides the serverless environment in which the REST API endpoints and background queue triggers are executed (IIE, 2026a; IIE, 2026b). Azure Table Storage is used to store coffee shop menu items and order logs, Azure File Share is used for staff documents, and Azure Storage Queues provide reliable asynchronous messaging during peak hours (IIE, 2026a; IIE, 2026b). Azurite provides a local development environment that emulates Azure Storage services without requiring the application to connect to a live Azure subscription during development (IIE, 2026a).   
JPEG
+ 4
System Architecture & Asynchronous Order Workflow
The CoffeeNChill system follows a layered approach in which different components are responsible for different parts of the application (IIE, 2026a). Postman is used as the client application to send HTTP requests to the CoffeeNChill API (IIE, 2026a). These requests are received by Azure Functions, where each Function is responsible for handling a specific operation such as menu management, document handling, or asynchronous order ingestion (IIE, 2026a; IIE, 2026b).   
JPEG
+ 3
During break times, hundreds of CoffeeNChill students place orders simultaneously; processing orders synchronously causes system crashes (IIE, 2026b). To introduce asynchronous order queuing and prevent system failures, the system follows a peak-hour asynchronous workflow (IIE, 2026b):   
JPEG
+ 1
Order Placement: When a student submits an order, the system does not block the client while waiting for cooking or barista preparation (IIE, 2026b). Instead, it places the order into an Azure Storage Queue named order-processing-queue via the producer endpoint POST /api/orders/queue (IIE, 2026b).   
JPEG
+ 1
Order Processing: An automated background Azure Function (ProcessOrderQueue) detects the incoming queue message using the [QueueTrigger("order-processing-queue")] attribute, parses the JSON payload string into an Order object, writes an entry into an Azure Table named Orders (with PartitionKey as OrderDate, RowKey as OrderId, and Status as "Received"), and simulates status transitions across the barista lifecycle: "Received" ➔ "Preparing" ➔ "Ready" ➔ "Collected" (IIE, 2026b).   
JPEG
The Azure Functions communicate with service classes that contain storage-related logic (IIE, 2026a). For example, MenuTableService communicates with the MenuItems table, DocumentStorageService handles staff documents, and queue processing services manage messaging and order status updates (IIE, 2026a; IIE, 2026b). During local development, these storage operations are directed to Azurite (IIE, 2026a).   
JPEG
+ 3
Changelog & Version History
v2.0 (Part 2 - Current Release):
Implemented asynchronous order queuing via POST /api/orders/queue (IIE, 2026b).   
JPEG
Added the ProcessOrderQueue background function triggered by order-processing-queue to automatically log orders into the Orders Azure Table and simulate barista lifecycle states (Received ➔ Preparing ➔ Ready ➔ Collected) (IIE, 2026b).   
JPEG
Configured multi-container orchestration using docker-compose.yml located in the root folder, pulling application images directly from Docker Hub (image: <dockerhub_username>/coffeeandchill-functions:v2.0) (IIE, 2026b).   
JPEG
Updated Postman collection tests to demonstrate messages being placed onto order-processing-queue and checking for status updates in the Orders Table (IIE, 2026b).   
JPEG
Added YouTube video link demonstrating single-command docker-compose up startup and successful Postman collection execution (IIE, 2026b).   
JPEG
v1.0 (Part 1 - Initial Release):
Built .NET 10 Azure Functions application using the isolated worker model (IIE, 2026a).   
JPEG
Implemented RESTful menu management endpoints (POST /api/menu, GET /api/menu, GET /api/menu/category/{category}, PUT /api/menu/{category}/{id}, DELETE /api/menu/{category}/{id}) linked to MenuTableService and MenuItems Azure Table Storage (IIE, 2026a).   
JPEG
Implemented staff document management endpoints (POST /api/documents/upload, GET /api/documents, GET /api/documents/download/{fileName}) linked to DocumentStorageService and Azure File Share (staff-docs) using stream-based file transfers (IIE, 2026a).   
JPEG
Integrated Azurite emulator for local storage and Docker containerisation (IIE, 2026a).   
JPEG
Menu Management
The menu management component provides a RESTful API for creating, viewing, updating and deleting menu items stored in an Azure Table named MenuItems (IIE, 2026a). Azure Table Storage uses PartitionKey (representing menu category) and RowKey (representing SKU/ID) to uniquely identify entities (IIE, 2026a).   
JPEG
+ 1
A typical menu item JSON payload is structured as follows (IIE, 2026a):   
JPEG
JSON
{
    "partitionKey": "Hot Drinks",
    "rowKey": "COF-001",
    "name": "Cappuccino",
    "description": "Espresso with steamed milk",
    "price": 35.00,
    "isAvailable": true
}
Validation ensures required fields are present and negative prices are rejected, returning standard HTTP response codes (201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error) (IIE, 2026a).   
JPEG
Staff Document Management
Staff documents are stored in an Azure File Share named staff-docs (IIE, 2026a). Endpoints include POST /api/documents/upload (using multipart/form-data), GET /api/documents, and GET /api/documents/download/{fileName} (IIE, 2026a). Stream-based transfers prevent loading large files entirely into memory (IIE, 2026a).   
JPEG
+ 2
Azure Storage and Azurite
Azurite is used as the local emulator for Azure Table, Queue, and Blob storage services during development (IIE, 2026a; IIE, 2026b). Local settings use UseDevelopmentStorage=true to connect the Functions app to Azurite without requiring an active Azure subscription (IIE, 2026a).   
JPEG
+ 2
Multi-Container Orchestration (Docker Compose)
In Part 2, local development services are combined into a single docker-compose.yml file located in the root folder of the repository (IIE, 2026b). Per mandatory rules, the compose file references published Docker Hub images rather than local build paths (IIE, 2026b):   
JPEG
+ 1
YAML
version: '3.8'

services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    container_name: azurite
    ports:
      - "10000:10000"
      - "10001:10001"
      - "10002:10002"
    volumes:
      - azurite_data:/data

  azure-functions:
    image: <dockerhub_username>/coffeeanchill-functions:v2.0
    container_name: coffeeanchill-app
    ports:
      - "8080:80"
    environment:
      - AzureWebJobsStorage=UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite
    depends_on:
      - azurite

volumes:
  azurite_data:
Postman Testing
Postman is used to verify API endpoints and queue workflows using environment variables such as {{baseUrl}} (IIE, 2026a; IIE, 2026b). The collection tests order enqueueing via POST /api/orders/queue and checks status transitions in the Orders Azure Table (IIE, 2026b).   
JPEG
+ 2
Project Structure
The repository is structured into clear components (IIE, 2026a):   
JPEG
Functions: Azure Functions for HTTP API endpoints and Queue triggers (IIE, 2026a; IIE, 2026b).   
JPEG
+ 1
Models: Data models including MenuItem and Order (IIE, 2026a; IIE, 2026b).   
JPEG
+ 1
Services: Storage and business logic (MenuTableService, DocumentStorageService) (IIE, 2026a).   
JPEG
Helpers: Utility classes (IIE, 2026a).   
JPEG
Root Folder: docker-compose.yml, host.json, local.settings.json, and Program.cs (IIE, 2026a; IIE, 2026b).   
JPEG
+ 1
Version Control and GitHub
Git and GitHub manage source control history across all team members, demonstrating clear individual commit histories (IIE, 2026a; IIE, 2026b). Supporting assets are stored in the docs directory (IIE, 2026a).   
JPEG
+ 2
Running the Application via Docker Compose
To start the orchestrated environment (IIE, 2026b):   
JPEG
Open terminal in the root folder containing docker-compose.yml (IIE, 2026b).   
JPEG
Run single command:
Bash
docker-compose up
Execute Postman collection requests to send orders to the queue and observe status progression in Azure Table Storage (IIE, 2026b).   
JPEG
Demonstration Video
YouTube Demonstration Link: [Insert YouTube Video Link Here] (IIE, 2026b)   
JPEG
(Demonstrates single-command docker-compose up startup and execution of Postman collection tests) (IIE, 2026b)
