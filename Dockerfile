FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore LogopedApp.sln
RUN dotnet publish ApiGateway/ApiGateway.csproj -c Release -o /app/ApiGateway
RUN dotnet publish UserService/UserService.csproj -c Release -o /app/UserService
RUN dotnet publish ProgressService/ProgressService.csproj -c Release -o /app/ProgressService
RUN dotnet publish ExerciseService/ExerciseService.csproj -c Release -o /app/ExerciseService

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000
CMD ["dotnet", "ApiGateway/ApiGateway.dll"]
