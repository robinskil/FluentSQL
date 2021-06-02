FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["TestFluentSql/TestFluentSql.csproj", "TestFluentSql/"]
RUN dotnet restore "TestFluentSql/TestFluentSql.csproj"
COPY . .
WORKDIR "/src/TestFluentSql"
ENTRYPOINT ["dotnet", "test"]

