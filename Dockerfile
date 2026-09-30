# ---------- Etapa de compilacion ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copiar solo los .csproj y restaurar
COPY src/ProductCatalog.Domain/ProductCatalog.Domain.csproj                 src/ProductCatalog.Domain/
COPY src/ProductCatalog.Application/ProductCatalog.Application.csproj       src/ProductCatalog.Application/
COPY src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj src/ProductCatalog.Infrastructure/
COPY src/ProductCatalog.Api/ProductCatalog.Api.csproj                       src/ProductCatalog.Api/
RUN dotnet restore src/ProductCatalog.Api/ProductCatalog.Api.csproj

# 2. Copiar el codigo y publicar
COPY src/ src/
RUN dotnet publish src/ProductCatalog.Api/ProductCatalog.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---------- Etapa final (solo runtime) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 8080

# Ejecutar sin privilegios de root
USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]