FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ApiAuthTokenGenerator.sln ./
COPY ./ApiAuthTokenGenerator/ApiAuthTokenGenerator.csproj ./ApiAuthTokenGenerator/
COPY ./ApiAuthTokenGenerator.Tests/ApiAuthTokenGenerator.Tests.csproj ./ApiAuthTokenGenerator.Tests/

RUN dotnet restore ./ApiAuthTokenGenerator/ApiAuthTokenGenerator.csproj
RUN dotnet restore ./ApiAuthTokenGenerator.Tests/ApiAuthTokenGenerator.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out ApiAuthTokenGenerator.Tests/ApiAuthTokenGenerator.Tests.csproj

CMD dotnet test
