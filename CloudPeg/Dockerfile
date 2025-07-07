#from linuxserver/ffmpeg as ffmpeg
#
#
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081


FROM alpine:latest AS base
RUN #ping google.com
RUN apk update 
RUN apk upgrade
RUN apk add --no-cache \
  dotnet9-sdk ffmpeg intel-media-driver nodejs

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
#RUN ufw status  
  
EXPOSE 8080
EXPOSE 8081
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CloudPeg/CloudPeg.csproj", "CloudPeg/"]
RUN dotnet restore "CloudPeg/CloudPeg.csproj"
RUN libman restore
COPY . .

WORKDIR "/src/CloudPeg/CloudPeg.UI"
RUN npm install
RUN npm build
RUN gulp --gulpfile ../gulpfile.mjs copyDist
WORKDIR "/src/CloudPeg"
RUN npm install
RUN dotnet build "./CloudPeg.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudPeg.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
EXPOSE 8080
EXPOSE 8081
EXPOSE 5000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudPeg.dll"]
