# CoffeeNChill – Cloud Development 2B

## Project Overview

CoffeeNChill is a cloud-based coffee shop management system developed as part of the CLDV6212 Cloud Development 2B module. The purpose of the project is to demonstrate the development of a cloud-ready application that provides RESTful API functionality for managing coffee shop menu items and staff documents. The application is developed using C# with .NET 10 and Azure Functions v4 using the .NET isolated worker model. Azure Storage services are used to provide persistent storage for the application's data, while Azurite is used as a local emulator during development and testing. The project also demonstrates containerisation using Docker, API testing using Postman, and version control using Git and GitHub.

## Technologies Used

The main technologies used in the development of CoffeeNChill are C#, .NET 10, Azure Functions v4, Azure Table Storage, Azure File Share, Azurite, Docker, Docker Hub, Postman, Visual Studio and GitHub. C# is used as the programming language for implementing the application's models, services and Azure Functions. .NET 10 provides the development framework, while Azure Functions provides the serverless environment in which the REST API endpoints are executed. Azure Table Storage is used to store coffee shop menu items, while Azure File Share is used for staff documents. Azurite provides a local development environment that emulates Azure Storage services without requiring the application to connect to a live Azure subscription during development.

## System Architecture

The CoffeeNChill system follows a layered approach in which different components are responsible for different parts of the application. Postman is used as the client application to send HTTP requests to the CoffeeNChill API. These requests are received by Azure Functions, where each Function is responsible for handling a specific operation such as creating, retrieving, updating or deleting a menu item. The Azure Functions then communicate with service classes that contain the storage-related logic. For example, the MenuTableService is responsible for communicating with the MenuItems table, while the DocumentStorageService is responsible for handling staff documents. This separation keeps the API functions focused on handling HTTP requests and responses while the service classes handle communication with storage. During local development, these storage operations are directed to Azurite, which acts as a local replacement for Azure Storage. The overall flow of the application can therefore be understood as Postman sending a request to an Azure Function, the Function passing the required operation to a storage service, and the storage service communicating with Azurite or Azure Storage to store or retrieve the requested information.

## Menu Management

The menu management component provides the coffee shop with a RESTful API for creating, viewing, updating and deleting menu items. Menu items are stored in an Azure Table called `MenuItems`. Each menu item contains a category, a unique item identifier or SKU, a name, description, price and availability status. Azure Table Storage uses a `PartitionKey` and `RowKey` to uniquely identify entities. In the CoffeeNChill application, the `PartitionKey` represents the menu category, such as `Hot Drinks`, while the `RowKey` represents the unique SKU or ID of the menu item, such as `COF-001`.

A typical menu item can be represented using the following JSON structure:

```json
{
    "partitionKey": "Hot Drinks",
    "rowKey": "COF-001",
    "name": "Cappuccino",
    "description": "Espresso with steamed milk",
    "price": 35.00,
    "isAvailable": true
}
```

The menu API provides five main endpoints. The `POST /api/menu` endpoint is used to create a new menu item. The `GET /api/menu` endpoint retrieves all menu items stored in the system. The `GET /api/menu/category/{category}` endpoint retrieves menu items belonging to a specific category. The `PUT /api/menu/{category}/{id}` endpoint is used to update an existing menu item, while the `DELETE /api/menu/{category}/{id}` endpoint removes a menu item from storage. These endpoints allow the menu to be managed through standard HTTP operations.

The CreateMenuItem Function is responsible for receiving new menu information through an HTTP POST request. The Function reads the JSON request body, converts it into a MenuItem object and performs validation before passing the item to the MenuTableService. The MenuTableService then stores the entity in the `MenuItems` table. The Function returns a `201 Created` response when the item has been successfully created. Validation is also included to ensure that important information such as the category, item ID and name is provided and that a negative price is not accepted.

The GetAllMenuItems Function is responsible for retrieving all menu items from the `MenuItems` table. It communicates with the MenuTableService, which queries the table and returns the stored entities. The Function then returns the results to the client as JSON with a successful `200 OK` response.

## Staff Document Management

CoffeeNChill also provides functionality for managing staff documents. Staff documents are stored in an Azure File Share called `staff-docs`. The document management functionality allows staff documents to be uploaded, listed and downloaded through HTTP API endpoints. The upload endpoint is `POST /api/documents/upload`, the listing endpoint is `GET /api/documents`, and the download endpoint is `GET /api/documents/download/{fileName}`.

The upload operation uses `multipart/form-data` so that a file can be submitted through an HTTP request. The application is designed to use stream-based file transfers so that documents can be transferred without unnecessarily loading the entire file into memory. The list operation provides information about available staff documents, including the file name, size and modified date. The download operation retrieves the requested document and streams it back to the client.

The document functionality is separated from the menu functionality through the use of the DocumentStorageService. This service is responsible for communicating with the configured file storage location, while the Azure Functions are responsible for handling the HTTP requests and responses. This separation improves the organisation and maintainability of the application.

## Azure Storage and Azurite

Azurite is used as the local Azure Storage emulator for the CoffeeNChill project. It allows the application to interact with storage services locally without requiring a live Azure Storage account during development. The Azurite environment is run independently in a Docker container named `coffeenchill-azurite`. The container exposes the storage service ports required by the application, including the Table Storage endpoint used by the MenuItems functionality.

The local Functions application uses the `UseDevelopmentStorage=true` configuration value in `local.settings.json` to indicate that development storage should be used. This allows the application to communicate with the local Azurite environment instead of connecting to a production Azure Storage account.

An example of the local configuration is:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
```

The use of Azurite makes development and testing easier because storage operations can be performed locally while still following the same general Azure Storage concepts that will be used when the application is deployed to Azure.

## Docker Containerisation

Docker is used to containerise the CoffeeNChill application and provide a consistent environment in which it can run. The project uses separate containers for the Azure Functions application and Azurite. This means that the Functions application and its local storage emulator can be started and managed independently rather than relying on Docker Compose.

The Azure Functions application is built into a Docker image using an official Azure Functions runtime image. The image is versioned using a semantic version such as `coffeenchill-functions:v1.0`. The Azurite environment is also provided as an independent container and can be tagged according to the project requirements.

The Docker approach allows the application to be packaged together with its runtime requirements, making it easier to move the application between different environments. Environment variables are used to provide configuration values to the Functions container rather than hard-coding environment-specific settings into the application.

## Postman Testing

Postman is used to test and demonstrate the functionality of the CoffeeNChill REST API. The Postman collection contains requests for the menu and document management endpoints. A `baseUrl` variable is used so that the same collection can be used with different environments without having to manually change every request URL.

For local development, the base URL points to the port displayed by the Azure Functions development environment, for example `http://localhost:7187`. The menu requests can then be accessed using URLs such as `{{baseUrl}}/api/menu`.

The API testing process includes both successful and unsuccessful requests. Successful requests are expected to return appropriate HTTP status codes such as `200 OK` when information is retrieved and `201 Created` when a new menu item is successfully created. Invalid requests should return `400 Bad Request`, while requests for resources that do not exist should return `404 Not Found`. Server-side problems are handled using appropriate `500 Internal Server Error` responses. Automated tests can be included in Postman to verify that the API returns the expected status codes and response information.

## Validation and Error Handling

Validation and error handling form an important part of the CoffeeNChill application. The application checks incoming data before attempting to store it. For menu items, validation is used to ensure that required information such as the category, unique item ID and name is provided. The application also prevents invalid values, such as negative menu prices, from being stored.

The Functions use HTTP status codes to communicate the result of requests to the client. A successful request returns an appropriate success status code, while invalid input results in a `400 Bad Request` response. When a requested menu item or document cannot be found, the application can return a `404 Not Found` response. Unexpected application or storage failures are handled through error handling and can return a `500 Internal Server Error` response.

## Project Structure

The CoffeeNChill project is organised into separate folders to make the application easier to maintain. The `Functions` folder contains the Azure Functions responsible for handling HTTP requests. These include the menu functions and staff document functions. The `Models` folder contains data models such as `MenuItem`, which represents the structure of a menu entity. The `Services` folder contains classes responsible for interacting with storage, including `MenuTableService` and `DocumentStorageService`. The `Helpers` folder is available for supporting utility functionality where required. Configuration and application startup are handled through files such as `Program.cs`, `host.json` and `local.settings.json`.

## Version Control and GitHub

Git and GitHub are used to manage the CoffeeNChill source code and demonstrate the development history of the project. Development is divided into meaningful commits so that changes can be tracked throughout the project. Examples of meaningful commits include adding the MenuItem model, implementing the Azure Table Storage service, creating the menu API functions, implementing document storage, adding Docker configuration, creating the Postman collection and updating project documentation.

Each group member should make meaningful contributions to the repository in accordance with the assignment requirements. The GitHub repository contains the application source code, documentation, Docker configuration, Postman collection and README file required for the project.

## Documentation

Additional documentation is maintained in the `docs` directory of the repository. This directory can contain the exported Postman collection, API documentation, Docker instructions and testing documentation. Keeping supporting documentation inside the repository makes it easier for lecturers, developers and other users to understand how the application was developed, configured, tested and deployed.

## Running the Application

To run the CoffeeNChill application locally, Docker Desktop should first be running. The Azurite container is then started independently so that the local storage services are available. The CoffeeNChill Functions project can then be opened in Visual Studio and built using the .NET development environment. Once the project has been started, Azure Functions displays the local HTTP address and port in the terminal. This address is then used as the base URL when testing the API through Postman.

For example, if the Functions terminal displays `http://localhost:7187`, the menu creation endpoint can be accessed using `POST http://localhost:7187/api/menu`. The exact port may differ between environments, so the URL displayed by the Functions runtime should be used.

## Current Development Status

The CoffeeNChill project has successfully been configured as a .NET 10 Azure Functions application using the isolated worker model. The MenuItem model and MenuTableService have been implemented, and the application has successfully connected to the local Azurite storage environment. The CreateMenuItem API has also been successfully tested through Postman, with a `201 Created` response confirming that a menu item can be submitted through the API and stored successfully.

Further development includes completing and testing the remaining menu CRUD endpoints, implementing the staff document functionality, completing the Postman collection and automated tests, configuring Docker deployment, publishing the required Docker image and completing the supporting documentation.

## Conclusion

CoffeeNChill demonstrates how cloud development technologies can be combined to create a practical coffee shop management system. The project uses C# and .NET 10 for application development, Azure Functions for serverless REST API functionality, Azure Storage for persistent data management and Azurite for local cloud-storage emulation. The application also demonstrates Docker containerisation, Postman API testing and GitHub version control. By separating HTTP functions from storage services and models, the system provides a structured and maintainable architecture that can be extended as additional functionality is implemented.

## Academic Information

**Module:** CLDV6212 – Cloud Development 2B
**Project:** CoffeeNChill
**Part:** Part 1
**Programming Language:** C#
**Framework:** .NET 10
**Cloud Platform:** Microsoft Azure
**Storage Emulator:** Azurite
**Containerisation:** Docker
**API Testing:** Postman
**Version Control:** Git and GitHub

## Team Members

* **[St10454156]** – [Fezokuhle Njabulo Cele]
* **[St10452327]** – [Siphesihle Siphamandla Bandile Nkabinde]
