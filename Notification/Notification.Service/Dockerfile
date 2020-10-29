#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Notification/Notification.Service/Notification.Service.csproj", "Notification/Notification.Service/"]
COPY ["Notification/EmailService/EmailService.csproj", "Notification/EmailService/"]
COPY ["Shared/Messaging.InterfacesConstants/Messaging.InterfacesConstants.csproj", "Shared/Messaging.InterfacesConstants/"]
RUN dotnet restore "Notification/Notification.Service/Notification.Service.csproj"
COPY . .
WORKDIR "/src/Notification/Notification.Service"
RUN dotnet build "Notification.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Service.dll"]
