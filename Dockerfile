# Use .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY *.sln .
COPY GymManagement/*.csproj GymManagement/
RUN dotnet restore

# Copy the rest of the code
COPY . .
WORKDIR /src/GymManagement
RUN dotnet publish -c Release -o /app/publish

# Use .NET 8 runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GymManagement.dll"]
