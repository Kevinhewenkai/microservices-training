FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers
# User of the same for every dockerfile in the project to take
# advantage of docker caching

COPY microservices-training.sln microservices-training.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj
COPY src/NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY test/AuctionServices.IntegrationTests/AuctionServices.IntegrationTests.csproj test/AuctionServices.IntegrationTests/AuctionServices.IntegrationTests.csproj
COPY test/AuctionServices.UnitTests/AuctionServices.UnitTests.csproj test/AuctionServices.UnitTests/AuctionServices.UnitTests.csproj

# Restore package deps
RUN dotnet restore microservices-training.sln

# Copy the app folders
COPY src/GatewayService src/GatewayService
COPY src/Contracts src/Contracts
WORKDIR /app/src/GatewayService
RUN dotnet publish -c Release -o /app/src/out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]