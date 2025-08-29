FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY *.sln .
COPY Fanex.Affiliate.Link/Fanex.Affiliate.Link.csproj ./Fanex.Affiliate.Link/
COPY Fanex.Affiliate.Link.Core/Fanex.Affiliate.Link.Core.csproj ./Fanex.Affiliate.Link.Core/

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

RUN dotnet restore -v n ./Fanex.Affiliate.Link/Fanex.Affiliate.Link.csproj

COPY Fanex.Affiliate.Link/. ./Fanex.Affiliate.Link/
COPY Fanex.Affiliate.Link.Core/. ./Fanex.Affiliate.Link.Core/

WORKDIR /src/Fanex.Affiliate.Link

RUN dotnet publish -v n -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

WORKDIR /app
COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Fanex.Affiliate.Link.dll"]