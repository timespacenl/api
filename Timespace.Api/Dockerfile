FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Timespace.Api/Timespace.Api.csproj", "Timespace.Api/"]
RUN dotnet restore "Timespace.Api/Timespace.Api.csproj"
COPY . .
WORKDIR "/src/Timespace.Api"
RUN dotnet build "Timespace.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Timespace.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Timespace.Api.dll"]
