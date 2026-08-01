FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Directory.*.props копируются ДО restore: без них central package management
# не найдёт версии пакетов и restore упадёт.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/HebrewLearnBot.Worker/HebrewLearnBot.Worker.csproj                 src/HebrewLearnBot.Worker/
COPY src/HebrewLearnBot.Infrastructure/HebrewLearnBot.Infrastructure.csproj src/HebrewLearnBot.Infrastructure/
COPY src/HebrewLearnBot.Application/HebrewLearnBot.Application.csproj       src/HebrewLearnBot.Application/
COPY src/HebrewLearnBot.Domain/HebrewLearnBot.Domain.csproj                 src/HebrewLearnBot.Domain/
RUN dotnet restore src/HebrewLearnBot.Worker/HebrewLearnBot.Worker.csproj

COPY . .
RUN dotnet publish src/HebrewLearnBot.Worker/HebrewLearnBot.Worker.csproj \
        -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update \
 && apt-get install -y --no-install-recommends tzdata locales curl \
 && rm -rf /var/lib/apt/lists/* \
 && sed -i 's/# ru_RU.UTF-8 UTF-8/ru_RU.UTF-8 UTF-8/' /etc/locale.gen \
 && locale-gen

# Без ICU сломается ToLowerInvariant и сортировка для иврита и кириллицы.
ENV TZ=Europe/Moscow \
    LANG=ru_RU.UTF-8 \
    LC_ALL=ru_RU.UTF-8 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080

WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "HebrewLearnBot.Worker.dll"]
