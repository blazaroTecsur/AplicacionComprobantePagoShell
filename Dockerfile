# =====================================
# BUILD
# =====================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG NUGET_USERNAME
ARG NUGET_TOKEN
ENV NUGET_USERNAME=${NUGET_USERNAME}
ENV NUGET_TOKEN=${NUGET_TOKEN}
ENV CI=true

WORKDIR /src

COPY nuget.config .
COPY . .

RUN dotnet restore

# Instalar libman y restaurar librer�as de cliente
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"
RUN cd ComprobantePago.Web && libman restore

RUN dotnet publish ComprobantePago.Web/ComprobantePago.Web.csproj \
    -c Release \
    -o /app/publish

# =====================================
# RUNTIME
# =====================================

FROM ghcr.io/sistecsur/dotnet-runtime:8.1

RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ComprobantePago.Web.dll"]