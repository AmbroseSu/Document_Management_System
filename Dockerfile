## Base image for runtime
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
#WORKDIR /app
#EXPOSE 8080 8443
#
## Copy HTTPS certificate into container
#USER root
#RUN mkdir -p /https
#COPY https/aspnetapp.pfx /https/aspnetapp.pfx
#RUN chmod 644 /https/aspnetapp.pfx
#
## Thiết lập biến môi trường
#ENV ASPNETCORE_ENVIRONMENT="Development"
#ENV ASPNETCORE_URLS="http://+:8080;https://+:8443"
#ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"
#ENV ASPNETCORE_Kestrel__Certificates__Default__Password="ambrosezen"
#
## Build stage
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["DocumentManagementSystemApplication/DocumentManagementSystemApplication.csproj", "DocumentManagementSystemApplication/"]
#COPY ["BusinessObject/BusinessObject.csproj", "BusinessObject/"]
#COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
#COPY ["Repository/Repository.csproj", "Repository/"]
#COPY ["Service/Service.csproj", "Service/"]
#RUN dotnet restore "./DocumentManagementSystemApplication/DocumentManagementSystemApplication.csproj"
#
## Copy the rest of the source files and build the project
#COPY . . 
#WORKDIR "/src/DocumentManagementSystemApplication"
#RUN dotnet build "./DocumentManagementSystemApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./DocumentManagementSystemApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "DocumentManagementSystemApplication.dll"]

# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Copy HTTPS certificate into container
USER root
RUN mkdir -p /https
COPY https/aspnetapp.pfx /https/aspnetapp.pfx
RUN chmod 644 /https/aspnetapp.pfx

# Thiết lập biến môi trường
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://+:8080;https://+:8443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="ambrosezen"

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file & restore dependencies trước để tận dụng cache
COPY DocumentManagementSystemApplication/*.csproj DocumentManagementSystemApplication/
COPY BusinessObject/*.csproj BusinessObject/
COPY DataAccess/*.csproj DataAccess/
COPY Repository/*.csproj Repository/
COPY Service/*.csproj Service/
COPY *.sln ./
RUN dotnet restore DocumentManagementSystemApplication/DocumentManagementSystemApplication.csproj
#RUN dotnet restore Service/Service.csproj

# Copy toàn bộ source code vào container
COPY . .
# Build project
WORKDIR /src/DocumentManagementSystemApplication
RUN dotnet publish DocumentManagementSystemApplication.csproj -c $BUILD_CONFIGURATION -o /app/publish

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5290 5291

ENTRYPOINT ["dotnet", "DocumentManagementSystemApplication.dll"]

