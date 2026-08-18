# =====================================
# BUILD
# =====================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore ComprobantePago.Web/ComprobantePago.Web.csproj

RUN dotnet publish ComprobantePago.Web/ComprobantePago.Web.csproj \
    -c Release \
    -o /app/publish

# =====================================
# RUNTIME
# =====================================

FROM ghcr.io/sistecsur/dotnet-runtime:8.1

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ComprobantePago.Web.dll"]