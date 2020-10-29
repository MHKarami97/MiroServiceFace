#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Web/Web/Web.csproj", "Web/Web/"]
COPY ["Shared/Messaging.InterfacesConstants/Messaging.InterfacesConstants.csproj", "Shared/Messaging.InterfacesConstants/"]
RUN dotnet restore "Web/Web/Web.csproj"
COPY . .
WORKDIR "/src/Web/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]
