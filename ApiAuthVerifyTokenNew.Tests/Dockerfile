FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ApiAuthVerifyToken.sln ./
COPY ./ApiAuthVerifyTokenNew/ApiAuthVerifyTokenNew.csproj ./ApiAuthVerifyTokenNew/
COPY ./ApiAuthVerifyTokenNew.Tests/ApiAuthVerifyTokenNew.Tests.csproj ./ApiAuthVerifyTokenNew.Tests/

RUN dotnet restore ApiAuthVerifyTokenNew
RUN dotnet restore ApiAuthVerifyTokenNew.Tests

# Copy everything else and build
COPY . ./

RUN dotnet build -c debug -o out ApiAuthVerifyTokenNew.Tests/ApiAuthVerifyTokenNew.Tests.csproj
CMD dotnet test ApiAuthVerifyTokenNew.Tests/ApiAuthVerifyTokenNew.Tests.csproj
