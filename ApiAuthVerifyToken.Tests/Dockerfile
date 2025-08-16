FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ApiAuthVerifyToken.sln ./
COPY ./ApiAuthVerifyToken/ApiAuthVerifyToken.csproj ./ApiAuthVerifyToken/
COPY ./ApiAuthVerifyToken.Tests/ApiAuthVerifyToken.Tests.csproj ./ApiAuthVerifyToken.Tests/

RUN dotnet restore ApiAuthVerifyToken
RUN dotnet restore ApiAuthVerifyToken.Tests

# Copy everything else and build
COPY . ./

RUN dotnet build -c debug -o out ApiAuthVerifyToken.Tests/ApiAuthVerifyToken.Tests.csproj
CMD dotnet test ApiAuthVerifyToken.Tests/ApiAuthVerifyToken.Tests.csproj
