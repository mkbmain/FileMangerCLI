FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["FileManagerCLI/FileManagerCLI.csproj", "FileManagerCLI/"]
RUN dotnet restore "FileManagerCLI/FileManagerCLI.csproj"
COPY . .
WORKDIR "/src/FileManagerCLI"
RUN dotnet build "FileManagerCLI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileManagerCLI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileManagerCLI.dll"]
