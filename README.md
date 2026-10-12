CoffeeNChill – Cloud Development (Part 2)
Project Overview
CoffeeNChill is a cloud-based coffee shop management system developed as part of the CLDV6212 Cloud Development module. The purpose of the project is to demonstrate the development of a cloud-ready application that provides RESTful API functionality for managing coffee shop menu items, staff documents, and an asynchronous order-processing queue workflow. The application is developed using C# with .NET 10 and Azure Functions v4 using the .NET isolated worker model. Azure Storage services are used to provide persistent storage for the application's data, while Azurite is used as a local emulator during development and testing. The project also demonstrates containerisation using Docker and Docker Compose, API testing using Postman, and version control using Git and GitHub.

Technologies Used
The main technologies used in the development of CoffeeNChill are C#, .NET 10, Azure Functions v4, Azure Table Storage, Azure File Share, Azure Storage Queues, Azurite, Docker, Docker Hub, Postman, Visual Studio and GitHub. C# is used as the programming language for implementing the application's models, services and Azure Functions. .NET 10 provides the development framework, while Azure Functions provides the serverless environment in which the REST API endpoints and queue triggers are executed. Azure Table Storage is used to store coffee shop menu items and processed order logs, Azure File Share is used for staff documents, and Azure Storage Queues handle asynchronous peak-hour order placement. Azurite provides a local development environment that emulates Azure Storage services without requiring a live Azure subscription during development.

System Architecture & Asynchronous Order Workflow
The CoffeeNChill system follows a layered approach in which different components are responsible for different parts of the application. Postman is used as the client application to send HTTP requests to the CoffeeNChill API. During peak hours, hundreds of students place orders simultaneously; to prevent system crashes caused by synchronous processing, an asynchronous queue workflow is introduced:

1. Order Placement: A student submits an order via the producer endpoint (POST /api/orders/queue), which serializes the order details into a JSON message string and pushes it into the order-processing-queueAzure Storage Queue without blocking the client. 
2. Order Processing: An automated background Azure Function (ProcessOrderQueue) detects messages on the queue via a [QueueTrigger("order-processing-queue")] attribute, parses the payload, logs an entry into the Orders Azure Table (with PartitionKey as OrderDate and RowKey as OrderId), and simulates the barista lifecycle (Received ➔ Preparing ➔ Ready ➔ Collected). 
Changelog & Version History
* v2.0 (Part 2 - Current Release): 
    * Implemented asynchronous order queuing via POST /api/orders/queue. 
    * Added the queue-triggered background function (ProcessOrderQueue) to handle automated lifecycle state transitions (Received ➔ Preparing ➔ Ready ➔ Collected) and record entries into the OrdersAzure Table. 
    * Configured multi-container orchestration using docker-compose.yml combining Azurite and published Docker Hub images (<dockerhub_username>/coffeeandchill-functions:v2.0). 
    * Expanded Postman collection and validation tests to cover queue publishing and order status retrieval. 
    * Added YouTube demonstration link for single-command docker-compose up startup and verification. 
* v1.0 (Part 1 - Initial Release): 
    * Configured .NET 10 Azure Functions application using the isolated worker model. 
    * Implemented menu management CRUD endpoints (POST /api/menu, GET /api/menu, GET /api/menu/category/{category}, PUT /api/menu/{category}/{id}, DELETE /api/menu/{category}/{id}) and MenuTableService interacting with the MenuItems table. 
    * Implemented staff document management endpoints (POST /api/documents/upload, GET /api/documents, GET /api/documents/download/{fileName}) using DocumentStorageService and Azure File Share (staff-docs). 
    * Established local development using Azurite (coffeenchill-azurite) and containerised the application using standalone Docker images. 
Menu Management
The menu management component provides the coffee shop with a RESTful API for creating, viewing, updating and deleting menu items. Menu items are stored in an Azure Table called MenuItems. Each menu item contains a category, a unique item identifier or SKU, a name, description, price and availability status. Azure Table Storage uses a PartitionKey and RowKey to uniquely identify entities. In the CoffeeNChill application, the PartitionKey represents the menu category (e.g., Hot Drinks), while the RowKey represents the unique SKU or ID of the menu item (e.g., COF-001).

A typical menu item can be represented using the following JSON structure:

JSON

{
    "partitionKey": "Hot Drinks",
    "rowKey": "COF-001",
    "name": "Cappuccino",
    "description": "Espresso with steamed milk",
    "price": 35.00,
    "isAvailable": true
}
Staff Document Management
CoffeeNChill also provides functionality for managing staff documents stored in an Azure File Share called staff-docs. The document management functionality allows staff documents to be uploaded via POST /api/documents/upload, listed via GET /api/documents, and downloaded via GET /api/documents/download/{fileName}. File transfers are stream-based to handle documents efficiently without loading entire files into memory.

Multi-Container Orchestration (Docker Compose)
For Part 2, the application services are orchestrated locally using Docker Compose, pulling images directly from Docker Hub.

Example docker-compose.yml:

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
    image: <dockerhub_username>/coffeeandchill-functions:v2.0
    container_name: coffeeandchill-app
    ports:
      - "8080:80"
    environment:
      - AzureWebJobsStorage=UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite
    depends_on:
      - azurite

volumes:
  azurite_data:
