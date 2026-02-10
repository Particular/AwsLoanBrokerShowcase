FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src

COPY . .

# Support different build contexts:
# - repo root context: project at src/Client
# - src folder context (common in IDE tooling): project at Client
RUN if [ -d "src/Client" ]; then PROJECT_PATH="src/Client"; else PROJECT_PATH="Client"; fi \
    && dotnet publish "$PROJECT_PATH" -c "$BUILD_CONFIGURATION" -o /app/publish --arch "$TARGETARCH" /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Client.dll"]
