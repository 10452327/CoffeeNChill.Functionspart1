FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CoffeeNChill.Functions.csproj", "./"]
RUN dotnet restore "CoffeeNChill.Functions.csproj"
COPY . .
RUN dotnet build "CoffeeNChill.Functions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoffeeNChill.Functions.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true