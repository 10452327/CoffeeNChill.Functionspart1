FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CoffeeNChill.Functionspart1.csproj", "./"]
RUN dotnet restore "CoffeeNChill.Functionspart1.csproj"
COPY . .
RUN dotnet build "CoffeeNChill.Functionspart1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoffeeNChill.Functionspart1.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true