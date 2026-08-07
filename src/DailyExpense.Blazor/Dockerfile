FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DailyExpenseManager.sln ./
COPY src/DailyExpense.Blazor/DailyExpense.Blazor.csproj src/DailyExpense.Blazor/
COPY src/DailyExpense.Contracts/DailyExpense.Contracts.csproj src/DailyExpense.Contracts/

RUN dotnet restore src/DailyExpense.Blazor/DailyExpense.Blazor.csproj

COPY src/ src/
RUN dotnet publish src/DailyExpense.Blazor/DailyExpense.Blazor.csproj -c Release -o /app/publish --no-restore

FROM nginx:1.27-alpine AS final
COPY src/DailyExpense.Blazor/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
