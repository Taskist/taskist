# syntax=docker/dockerfile:1

# ----- build -----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# restore first so the layer is cached until a project file changes
COPY src/Taskist.sln ./
COPY src/Libraries/Taskist.Core/Taskist.Core.csproj Libraries/Taskist.Core/
COPY src/Libraries/Taskist.Data/Taskist.Data.csproj Libraries/Taskist.Data/
COPY src/Libraries/Taskist.Service/Taskist.Service.csproj Libraries/Taskist.Service/
COPY src/Presentation/Taskist.Web/Taskist.Web.csproj Presentation/Taskist.Web/

# restore the web project, not the solution: the solution also lists the test project, which is not copied here
RUN dotnet restore Presentation/Taskist.Web/Taskist.Web.csproj

COPY src/ ./

RUN dotnet publish Presentation/Taskist.Web/Taskist.Web.csproj \
    -c Release \
    -o /app \
    --no-restore

# ----- runtime -----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl backs the container health check
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

# data protection keys must outlive the container (attachments live in the database)
RUN mkdir -p /app/App_Data/DataProtectionKeys \
    && chown -R $APP_UID:$APP_UID /app/App_Data

# run as the non-root user provided by the base image
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Taskist.Web.dll"]
