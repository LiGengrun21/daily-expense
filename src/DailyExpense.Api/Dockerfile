FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DailyExpenseManager.sln ./
COPY src/DailyExpense.Api/DailyExpense.Api.csproj src/DailyExpense.Api/
COPY src/DailyExpense.Application/DailyExpense.Application.csproj src/DailyExpense.Application/
COPY src/DailyExpense.Contracts/DailyExpense.Contracts.csproj src/DailyExpense.Contracts/
COPY src/DailyExpense.Domain/DailyExpense.Domain.csproj src/DailyExpense.Domain/
COPY src/DailyExpense.Infrastructure/DailyExpense.Infrastructure.csproj src/DailyExpense.Infrastructure/

RUN dotnet restore src/DailyExpense.Api/DailyExpense.Api.csproj

COPY src/ src/
RUN dotnet publish src/DailyExpense.Api/DailyExpense.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DailyExpense.Api.dll"]
