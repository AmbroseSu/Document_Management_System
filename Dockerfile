# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Cài đặt các gói cần thiết cho LibreOffice và fonts
RUN apt-get update && apt-get install -y --no-install-recommends \
    libreoffice-common \
    libreoffice-core \
    libreoffice-writer \
    libreoffice-calc \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*

# Copy font Times New Roman
COPY ./fonts/TimesNewRoman.ttf /usr/share/fonts/truetype/msttcorefonts/TimesNewRoman.ttf
RUN fc-cache -fv

# Thiết lập biến môi trường
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://+:5290"

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file & restore
COPY DocumentManagementSystemApplication/*.csproj DocumentManagementSystemApplication/
COPY BusinessObject/*.csproj BusinessObject/
COPY DataAccess/*.csproj DataAccess/
COPY Repository/*.csproj Repository/
COPY Service/*.csproj Service/
COPY *.sln ./

RUN dotnet restore DocumentManagementSystemApplication/DocumentManagementSystemApplication.csproj

# Copy full source
COPY . .
WORKDIR /src/DocumentManagementSystemApplication
RUN dotnet publish DocumentManagementSystemApplication.csproj -c $BUILD_CONFIGURATION -o /app/publish

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5290

ENTRYPOINT ["dotnet", "DocumentManagementSystemApplication.dll"]
