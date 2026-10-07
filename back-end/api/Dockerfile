FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet publish back-end.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "back-end.dll"]
