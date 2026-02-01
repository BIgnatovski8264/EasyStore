FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY EasyStore.API/*.csproj ./EasyStore.API/
COPY EasyStore.Data/*.csproj ./EasyStore.Data/
COPY EasyStore.Domain/*.csproj ./EasyStore.Domain/
COPY EasyStore.Core/*.csproj ./EasyStore.Core/
COPY EasyStore.Common/*.csproj ./EasyStore.Common/

RUN dotnet restore

COPY . .
RUN dotnet publish EasyStore.API/EasyStore.API.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Template.API.dll"]