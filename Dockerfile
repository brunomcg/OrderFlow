# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto
COPY ["OrderFlow.API/OrderFlow.API.csproj", "OrderFlow.API/"]
COPY ["OrderFlow.Application/OrderFlow.Application.csproj", "OrderFlow.Application/"]
COPY ["OrderFlow.Domain/OrderFlow.Domain.csproj", "OrderFlow.Domain/"]
COPY ["OrderFlow.Infrastructure/OrderFlow.Infrastructure.csproj", "OrderFlow.Infrastructure/"]

RUN dotnet restore "OrderFlow.API/OrderFlow.API.csproj"

COPY . .
RUN dotnet publish "OrderFlow.API/OrderFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Instalação crítica de dependências para o Runtime .NET em Alpine
# A falta destas causa o erro 139 (Segmentation Fault)
RUN apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libstdc++ \
    zlib

# Configurações de porta e ambiente
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.API.dll"]