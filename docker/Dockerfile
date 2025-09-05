# Stage 1: Build the application
FROM alpine:latest AS build

RUN apk update && \
    apk upgrade && \
    apk add --no-cache \
    dotnet9-sdk \
    nodejs \
    git \
    npm && \
    rm -rf /var/cache/apk/*

ENV PATH="/root/.dotnet/tools:/usr/local/bin:${PATH}"

RUN npm install -g gulp-cli
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

WORKDIR /src
RUN git init .

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
run   git config --global user.email "you@example.com"
run      git config --global user.name "Your Name"

run git add .
run git commit -m"debug"
WORKDIR "/src/CloudPeg"
RUN dotnet publish "./CloudPeg.csproj" -c Release -o /app/publish_output /p:UseAppHost=false


FROM fedora:latest AS final

# Install required packages using dnf
# 'dnf' is the package manager for Fedora.
# 'dnf install -y' is the equivalent of 'apt-get install -y'.
# The package names are different and will need to be looked up.
RUN dnf update -y && \
    dnf install -y \
    wget \
    gnupg2 \
    ffmpeg && \
    dnf clean all

# To install Intel Media VA Driver, you'll need to enable the RPM Fusion non-free repository.
# This is the Fedora equivalent of adding a PPA or custom repository.
RUN dnf install -y \
    https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-$(rpm -E %fedora).noarch.rpm \
    https://download1.rpmfusion.org/nonfree/fedora/rpmfusion-nonfree-release-$(rpm -E %fedora).noarch.rpm && \
    dnf update -y && \
    dnf install -y \
    intel-media-driver && \
    dnf clean all

# For the .NET runtime, you will need to add the Microsoft package repository for Fedora.
RUN rpm -Uvh https://packages.microsoft.com/config/fedora/39/packages-microsoft-prod.rpm && \
    dnf install -y \
    dotnet-sdk-9.0 && \
    dnf clean all

EXPOSE 5000

WORKDIR /app

# The 'build' stage is not included in your script, but this line assumes it exists.
# We keep this as-is, as it's a standard Docker command.
COPY --from=build /app/publish_output .

RUN if [ -d /app/fsroot ]; then ln -s /app/fsroot /fsroot; else echo "Warning: /app/fsroot not found in published output, skipping symlink."; fi

ENTRYPOINT ["dotnet", "CloudPeg.dll"]