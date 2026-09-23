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

# Instalar fonts-liberation en la etapa build (tiene root) para extraer el TTF
RUN apt-get update && apt-get install -y --no-install-recommends fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# =====================================
# RUNTIME
# =====================================

FROM ghcr.io/sistecsur/dotnet-runtime:8.1

WORKDIR /app

COPY --from=build /app/publish .

# Copiar solo el archivo TTF desde la etapa build (sin necesitar apt en runtime)
COPY --from=build /usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf ./fonts/LiberationMono-Regular.ttf

ENTRYPOINT ["dotnet", "ComprobantePago.Web.dll"]