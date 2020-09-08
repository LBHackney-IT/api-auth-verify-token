FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ApiAuthTokenManagement.sln ./
COPY ./ApiAuthTokenManagement/ApiAuthTokenManagement.csproj ./ApiAuthTokenManagement/
COPY ./ApiAuthTokenManagement.Tests/ApiAuthTokenManagement.Tests.csproj ./ApiAuthTokenManagement.Tests/

RUN dotnet restore ./ApiAuthTokenManagement/ApiAuthTokenManagement.csproj
RUN dotnet restore ./ApiAuthTokenManagement.Tests/ApiAuthTokenManagement.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out ApiAuthTokenManagement.Tests/ApiAuthTokenManagement.Tests.csproj

CMD dotnet test
