ARG CI_REGISTRY_IMAGE

FROM $CI_REGISTRY_IMAGE:builder AS deps

RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

ENV NUGET_PACKAGES=/src/packages

COPY --from=deps $NUGET_PACKAGES/. $NUGET_PACKAGES/

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["CRM.BackgroundJob/CRM.BackgroundJob.csproj", "CRM.BackgroundJob/"]
COPY ["CRM.BackgroundJob.Core/CRM.BackgroundJob.Core.csproj", "CRM.BackgroundJob.Core/"]

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

RUN dotnet restore -v n -r linux-x64 "CRM.BackgroundJob/CRM.BackgroundJob.csproj"

COPY . .

WORKDIR "/src/CRM.BackgroundJob"

RUN dotnet publish CRM.BackgroundJob.csproj -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine-25h1 AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "CRM.BackgroundJob.dll"]
