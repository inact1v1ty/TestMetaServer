FROM microsoft/dotnet-framework:4.7.2-sdk AS build
WORKDIR /app

# Expose the ports
EXPOSE 80
EXPOSE 443

# Run restore
COPY TestMetaServer.sln .
COPY TestMetaServer/*.csproj ./TestMetaServer/
RUN dotnet restore

# Run build
COPY . .
WORKDIR /app/TestMetaServer
RUN dotnet build

# Run publish
FROM build as publish
WORKDIR /app/TestMetaServer
RUN dotnet publish -c Release -o out

# Run the server
FROM microsoft/dotnet-framework:4.7.2-runtime AS runtime
WORKDIR /app
COPY --from=publish /app/TestMetaServer/out ./
ENTRYPOINT ["TestMetaServer.exe"]