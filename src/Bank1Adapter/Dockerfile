FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bank1Adapter/Bank1Adapter.csproj", "Bank1Adapter/"]
COPY ["BankMessages/BankMessages.csproj", "BankMessages/"]
COPY ["CommonConfigurations/CommonConfigurations.csproj", "CommonConfigurations/"]
RUN dotnet restore "Bank1Adapter/Bank1Adapter.csproj"
COPY . .
WORKDIR "/src/Bank1Adapter"
RUN dotnet build "Bank1Adapter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bank1Adapter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bank1Adapter.dll"]
