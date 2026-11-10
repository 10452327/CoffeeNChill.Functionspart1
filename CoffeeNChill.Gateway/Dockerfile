# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the Gateway project file
COPY ["CoffeeNChill.Gateway.csproj", "./"]

# Restore dependencies
RUN dotnet restore

# Copy the remaining source files
COPY . .

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# ASP.NET Core listens on port 8080
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CoffeeNChill.Gateway.dll"]