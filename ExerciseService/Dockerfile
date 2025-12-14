# Базовий образ із повним SDK .NET 9 — використовується для збирання проєкту
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Встановлюємо кореневу робочу директорію всередині контейнера
WORKDIR /src

# Копіюємо файл проекту (.csproj) — це дозволяє кешувати restore
COPY *.csproj .

# Відновлюємо NuGet-пакети
RUN dotnet restore

# Копіюємо весь вихідний код у контейнер
COPY . .

# Публікуємо застосунок у Release-конфігурації в окрему папку
RUN dotnet publish -c Release -o /app/publish

# Мінімальний рантайм-образ .NET 9 для запуску застосунку
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Встановлюємо робочу директорію
WORKDIR /app

# Копіюємо результати публікації з попереднього етапу
COPY --from=build /app/publish .

# Вказуємо контейнеру слухати порт 80
ENV ASPNETCORE_URLS=http://+:80

# Вказуємо команду запуску сервісу
ENTRYPOINT ["dotnet", "ExerciseService.dll"]
