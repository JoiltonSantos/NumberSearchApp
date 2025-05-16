FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NumberSearchApp.Application/NumberSearchApp.Application.csproj", "NumberSearchApp.Application/"]
COPY ["NumberSearchApp.Domain/NumberSearchApp.Domain.csproj", "NumberSearchApp.Domain/"]
COPY ["NumberSearchApp.Infrastructure/NumberSearchApp.Infrastructure.csproj", "NumberSearchApp.Infrastructure/"]
RUN dotnet restore "NumberSearchApp.Application/NumberSearchApp.Application.csproj"
COPY . .
WORKDIR "/src/NumberSearchApp.Application"
RUN dotnet build "NumberSearchApp.Application.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NumberSearchApp.Application.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NumberSearchApp.Application.dll"]