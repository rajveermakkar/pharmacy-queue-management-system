# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PharmacyQueue/PharmacyQueue.csproj", "PharmacyQueue/"]
RUN dotnet restore "PharmacyQueue/PharmacyQueue.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "PharmacyQueue/PharmacyQueue.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Copy the published app
COPY --from=build /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "PharmacyQueue.dll"]

# Add metadata
LABEL maintainer="Pharmacy Queue Team"
LABEL version="1.0.0"
LABEL description="Pharmacy Queue Management System"