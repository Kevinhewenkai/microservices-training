FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers
# User of the same for every dockerfile in the project to take
# advantage of docker caching

COPY microservices-training.sln microservices-training.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# Restore package deps
RUN dotnet restore microservices-training.sln

# Copy the app folders
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts
WORKDIR /app/src/AuctionService
RUN dotnet publish -c Release -o /app/src/out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]