# Stage 1: Build the application
FROM alpine:latest AS build

RUN apk update && \
    apk upgrade && \
    apk add --no-cache \
    dotnet9-sdk \
    nodejs \
    npm && \
    rm -rf /var/cache/apk/*

ENV PATH="/root/.dotnet/tools:/usr/local/bin:${PATH}"

RUN npm install -g gulp-cli
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

WORKDIR /src

COPY ["CloudPeg/CloudPeg.csproj", "CloudPeg/"]
RUN dotnet restore "CloudPeg/CloudPeg.csproj"

COPY . .

WORKDIR "/src/CloudPeg"
RUN libman restore
RUN npm install

WORKDIR "/src/CloudPeg/CloudPeg.UI"
RUN npm install
RUN npm run build
RUN npx gulp --gulpfile ../gulpfile.mjs copyDist

WORKDIR "/src/CloudPeg"
RUN dotnet publish "./CloudPeg.csproj" -c Release -o /app/publish_output /p:UseAppHost=false


# Stage 2: Create the smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

RUN apk update && \
    apk add --no-cache \
    ffmpeg \
    intel-media-driver && \
    rm -rf /var/cache/apk/*

EXPOSE 8080
EXPOSE 8081
EXPOSE 5000

WORKDIR /app

COPY --from=build /app/publish_output .

RUN if [ -d /app/fsroot ]; then ln -s /app/fsroot /fsroot; else echo "Warning: /app/fsroot not found in published output, skipping symlink."; fi

ENTRYPOINT ["dotnet", "CloudPeg.dll"]