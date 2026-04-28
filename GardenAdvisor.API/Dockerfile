FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GardenAdvisor.API/GardenAdvisor.API.csproj GardenAdvisor.API/
RUN dotnet restore GardenAdvisor.API/GardenAdvisor.API.csproj

COPY GardenAdvisor.API/ GardenAdvisor.API/
RUN dotnet publish GardenAdvisor.API/GardenAdvisor.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "GardenAdvisor.API.dll"]
