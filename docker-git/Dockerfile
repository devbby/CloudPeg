# Stage 1: Build the application
FROM alpine:latest AS build

RUN apk update && \
    apk upgrade && \
    apk add --no-cache \
    dotnet9-sdk \
    nodejs \
    npm \
    git &&  \
    rm -rf /var/cache/apk/*

ENV PATH="/root/.dotnet/tools:/usr/local/bin:${PATH}"

RUN npm install -g gulp-cli
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

WORKDIR /src

RUN git clone --branch main --single-branch "https://github.com/devbby/CloudPeg.git"
run ls CloudPeg/ -la 
RUN dotnet restore "CloudPeg/CloudPeg/CloudPeg.csproj"
 
WORKDIR "/src/CloudPeg/CloudPeg"
RUN libman restore
RUN npm install

WORKDIR "/src/CloudPeg/CloudPeg/CloudPeg.UI"
RUN npm install
RUN npm run build
RUN npx gulp --gulpfile ../gulpfile.mjs copyDist

WORKDIR "/src/CloudPeg/CloudPeg"
RUN dotnet publish "./CloudPeg.csproj" -c Release -o /app/publish_output /p:UseAppHost=false

# Stage 2: Create the smaller runtime image
# Use a Fedora-based .NET runtime image.
# You can choose a minimal one like 'fedora-minimal' or a full 'fedora'.
# For this example, let's use 'fedora-minimal' to keep it small, but you'll need to add .NET.
# A more direct approach is to use a .NET image built for Fedora, if available.
# As of now, Microsoft provides .NET images based on a variety of distros, but direct Fedora images are less common than Debian/Ubuntu.
# A common practice is to use a base Fedora image and install the .NET runtime yourself.

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

