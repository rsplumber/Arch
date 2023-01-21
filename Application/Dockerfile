FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Arch/Arch.csproj", "Arch/"]
RUN dotnet restore "Arch/Arch.csproj"
COPY . .
WORKDIR "/src/Arch"
RUN dotnet build "Arch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Arch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Arch.dll"]
