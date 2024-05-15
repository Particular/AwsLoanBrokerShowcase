FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LoanBroker/LoanBroker.csproj", "LoanBroker/"]
COPY ["BankMessages/BankMessages.csproj", "BankMessages/"]
COPY ["ClientMessages/ClientMessages.csproj", "ClientMessages/"]
COPY ["CommonConfigurations/CommonConfigurations.csproj", "CommonConfigurations/"]
RUN dotnet restore "LoanBroker/LoanBroker.csproj"
COPY . .
WORKDIR "/src/LoanBroker"
RUN dotnet build "LoanBroker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LoanBroker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoanBroker.dll"]
