FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5229

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
RUN mkdir "outp"

COPY ["./." , "outp/"]

RUN dotnet dev-certs https --clean

RUN dotnet restore "outp/Application/Application.csproj"

RUN dotnet build "outp/Application/Application.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "outp/Application/Application.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application.dll"]

