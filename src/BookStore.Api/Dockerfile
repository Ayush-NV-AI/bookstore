# Build context: src/BookStore.Api   (docker compose builds it for you)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BookStore.Api.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# The aspnet image ships a non-root user; never run the API as root.
USER $APP_UID
ENTRYPOINT ["dotnet", "BookStore.Api.dll"]
