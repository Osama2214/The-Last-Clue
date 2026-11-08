FROM mcr.microsoft.com/dotnet/sdk:10.0.401-noble@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317 AS build
WORKDIR /source
COPY global.json Directory.Build.props Directory.Packages.props NuGet.Config ./
COPY src/LastClue.Application/LastClue.Application.csproj src/LastClue.Application/packages.lock.json src/LastClue.Application/
COPY src/LastClue.Infrastructure/LastClue.Infrastructure.csproj src/LastClue.Infrastructure/packages.lock.json src/LastClue.Infrastructure/
COPY src/LastClue.Workers/LastClue.Workers.csproj src/LastClue.Workers/packages.lock.json src/LastClue.Workers/
COPY src/LastClue.Web/LastClue.Web.csproj src/LastClue.Web/packages.lock.json src/LastClue.Web/
RUN dotnet restore src/LastClue.Web/LastClue.Web.csproj --locked-mode
COPY src/ src/
RUN dotnet publish src/LastClue.Web/LastClue.Web.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble@sha256:1a48394b6f29d3a32c79d9edecd4abda0ecbe8776d567dbfc4590ed6979c740c AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R "$APP_UID:$APP_UID" /home/app/.aspnet
COPY --from=build /app/publish ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=5s --start-period=15s --retries=3 \
    CMD curl --fail --silent --show-error --max-time 3 http://127.0.0.1:8080/health || exit 1
ENTRYPOINT ["dotnet", "LastClue.Web.dll"]
