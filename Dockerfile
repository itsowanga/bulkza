# syntax=docker/dockerfile:1
#
# One Dockerfile for both apps. docker-compose.yml picks the target and the project:
#   target api    + PROJECT=Api     -> ASP.NET Core Web API
#   target worker + PROJECT=Worker  -> outbox publisher
#
# No secrets are baked into the image. Configuration arrives through environment variables at run time.

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Restore first so the package layer is cached until a project file changes.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj src/Api/
COPY src/Worker/Worker.csproj src/Worker/

ARG PROJECT
RUN dotnet restore src/${PROJECT}/${PROJECT}.csproj

COPY src/ src/
# Warnings are not errors here: Release turns them into errors in Directory.Build.props, and the image build
# should not fail on warnings such as NuGet audit notices. The test run and local builds still show them.
RUN dotnet publish src/${PROJECT}/${PROJECT}.csproj -c Release --no-restore -p:TreatWarningsAsErrors=false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS api
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS worker
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Worker.dll"]
