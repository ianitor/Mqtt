FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY NuGet.Config /root/.nuget/NuGet/
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><Salzburgdev><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></Salzburgdev></packageSourceCredentials></configuration>|" /root/.nuget/NuGet/NuGet.Config
COPY ["MqttBenchmark/MqttBenchmark.csproj", "MqttBenchmark/"]
RUN dotnet restore "MqttBenchmark/MqttBenchmark.csproj"
COPY . .
WORKDIR "/src/MqttBenchmark"
RUN dotnet build "MqttBenchmark.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MqttBenchmark.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MqttBenchmark.dll"]
