# CoffeeNChill – Cloud Development (Part 3 / POE)

## Project Overview
CoffeeNChill is a cloud-based coffee shop management system developed as part of the CLDV6212 Cloud Development module (IIE, 2026a; IIE, 2026c)[cite: 1, 3]. The purpose of the project is to provide unified RESTful API functionality for managing coffee shop menu items, staff documents, and an asynchronous order-processing queue workflow (IIE, 2026a; IIE, 2026b)[cite: 1, 2]. In Part 3, access to CoffeeNChill services is unified by building an ASP.NET Core Web API Gateway secured with JSON Web Token (JWT) authentication and Role-Based Access Control (RBAC), storing user profiles in an Azure Table, deploying the complete stack to a live Linux VPS host, and optionally automating deployment via GitHub Actions CI/CD pipelines (IIE, 2026c; IIE, 2026d)[cite: 3, 4].

## Live VPS Deployment URL
* **Production Gateway Swagger UI:** `http://<your-vps-ip>:5000/swagger` (IIE, 2026d)

## Technologies Used
The main technologies used in the development of CoffeeNChill are C#, .NET 10, ASP.NET Core Web API, Azure Functions v4, Azure Table Storage, Azure File Share, Azure Storage Queues, Azurite, Docker, Docker Hub, Swashbuckle (Swagger/OpenAPI), BCrypt, GitHub Actions, Linux VPS hosting, Postman, Visual Studio, and GitHub (IIE, 2026a; IIE, 2026b; IIE, 2026c; IIE, 2026d)[cite: 1, 2, 3, 4]. C# is used as the programming language for implementing models, services, authentication controllers, and Azure Functions (IIE, 2026a; IIE, 2026c)[cite: 1, 3]. .NET 10 provides the core development framework, while ASP.NET Core Web API acts as the unified gateway (IIE, 2026c). Azure Table Storage maintains menu items, order logs, and user profile credentials (`Users` table), Azure File Share handles staff documents (`staff-docs`), and Azure Storage Queues handle peak-hour asynchronous messaging (`order-processing-queue`) (IIE, 2026a; IIE, 2026b; IIE, 2026c)[cite: 1, 2, 3]. Azurite provides a local development emulator (IIE, 2026a; IIE, 2026b)[cite: 1, 2].

## System Architecture & Unified Gateway Workflow
The CoffeeNChill system follows a secure, layered gateway architecture (IIE, 2026c)[cite: 3]. Client applications (such as Postman, Swagger UI, or web portals) send HTTP requests to the ASP.NET Core Web API Gateway rather than communicating directly with backend functions (IIE, 2026c)[cite: 3]. 
* **Public Access:** Students can view menu items and place orders without logging in (IIE, 2026c)[cite: 3].
* **Staff/Admin Access:** Only authenticated baristas and canteen managers holding valid JWT Bearer Tokens can modify menu prices, upload operational files, or update order preparation statuses (IIE, 2026c)[cite: 3].

### User Profiles & Azure Table Storage (`Users`)
User profiles are persisted in an Azure Table named `Users` (IIE, 2026c)[cite: 3]:
* **PartitionKey:** Represents the user role (`"Admin"`, `"Staff"`, or `"Customer"`) (IIE, 2026c)[cite: 3].
* **RowKey:** Represents the user email or username (e.g., `barista@coffeeandchill.ac.za`) (IIE, 2026c)[cite: 3].
* **PasswordHash:** Securely hashed password using BCrypt, PBKDF2, or Identity PasswordHasher (IIE, 2026c)[cite: 3].
* **FullName & CreatedDate:** Additional user metadata (IIE, 2026c)[cite: 3].

## Changelog & Version History
* **v3.0 (Part 3 / POE - Current Release):**
  * Built an ASP.NET Core Web API Gateway unifying access to all backend services (IIE, 2026c)[cite: 3].
  * Integrated interactive Swashbuckle Swagger/OpenAPI UI with XML documentation enabled for endpoints, request bodies, and response codes (IIE, 2026c)[cite: 3].
  * Implemented JWT Bearer Token authentication and RBAC policies, storing hashed user profiles in the `Users` Azure Table via `POST /api/auth/register` and `POST /api/auth/login` (IIE, 2026c; IIE, 2026d)[cite: 3, 4].
  * Deployed the complete multi-container stack to a live Linux VPS host using Docker Compose pulling published images from Docker Hub (IIE, 2026d).
  * Configured an automated CI/CD pipeline (`.github/workflows/deploy.yml`) using GitHub Actions to automate testing, building, Docker Hub pushing, and VPS deployment (IIE, 2026d).
  * Updated Postman collection to support Bearer `{{token}}` authentication flows (IIE, 2026e).
  * Added YouTube video demonstration link showcasing live VPS execution, Swagger UI testing, and RBAC authorization (IIE, 2026d; IIE, 2026e).

* **v2.0 (Part 2):**
  * Implemented asynchronous order queuing via `POST /api/orders/queue` (IIE, 2026b).
  * Added the `ProcessOrderQueue` background function for automated order lifecycle management (`Received` ➔ `Preparing` ➔ `Ready` ➔ `Collected`) and logging into the `Orders` Azure Table (IIE, 2026b)[cite: 2].
  * Configured multi-container orchestration with Docker Compose using Docker Hub images (IIE, 2026b)[cite: 2].

* **v1.0 (Part 1):**
  * Configured .NET 10 Azure Functions application using the isolated worker model (IIE, 2026a).
  * Implemented menu management CRUD endpoints (`MenuItems` table) and staff document storage (`staff-docs` file share) with Azurite emulation (IIE, 2026a)[cite: 1].

## Authentication Endpoints & RBAC Policies (`/api/auth`)
* **`POST /api/auth/register`:** Hashes user passwords and creates user records in the `Users` Azure Table (IIE, 2026d).
* **`POST /api/auth/login`:** Validates credentials against the `Users` table and returns a signed JWT Bearer Token containing `ClaimTypes.Name`, `ClaimTypes.Email`, and `ClaimTypes.Role` (IIE, 2026d).

### Role-Based Authorization Policies:
* **`[AllowAnonymous]`:** `GET /api/menu`, `POST /api/orders/queue`, `POST /api/auth/register`, `POST /api/auth/login` (IIE, 2026d).
* **`[Authorize(Roles = "Admin,Staff")]`:** `POST /PUT/DELETE /api/menu`, `POST/GET/DELETE /api/documents`, `PUT /api/orders/status` (IIE, 2026d).

## Menu Management & Staff Document Management
* **Menu Management:** RESTful API for creating, viewing, updating, and deleting menu items stored in the `MenuItems` table using category `PartitionKey` and SKU `RowKey` (IIE, 2026a)[cite: 1]. Validated against negative pricing (IIE, 2026a)[cite: 1].
* **Staff Document Management:** Stream-based file uploads (`multipart/form-data`), listing, and downloads stored in the `staff-docs` Azure File Share (IIE, 2026a)[cite: 1].

## Multi-Container Orchestration & VPS Deployment
Local development and production hosting are managed via Docker Compose, pulling pre-built version-tagged images directly from Docker Hub (IIE, 2026b; IIE, 2026d)[cite: 2, 4]:

```yaml
version: '3.8'

services:
  azurite:
    image: [mcr.microsoft.com/azure-storage/azurite](https://mcr.microsoft.com/azure-storage/azurite)
    container_name: azurite
    ports:
      - "10000:10000"
      - "10001:10001"
      - "10002:10002"
    volumes:
      - azurite_data:/data

  api-gateway:
    image: <dockerhub_username>/coffeeandchill-gateway:v3.0
    container_name: coffeeandchill-gateway
    ports:
      - "5000:80"
    environment:
      - AzureWebJobsStorage=UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite
      - Jwt__Key=SuperSecretKeyForCoffeeNChill2026!
      - Jwt__Issuer=CoffeeNChillGateway
      - Jwt__Audience=CoffeeNChillClients
    depends_on:
      - azurite

volumes:
  azurite_data:
