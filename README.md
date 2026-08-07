# Daily Expense Manager

个人日常支出管理应用，用于练习 C#、ASP.NET Core Web API、Blazor、EF Core、PostgreSQL、xUnit、Docker、GitHub Actions、Terraform、Azure Container Apps、OpenTelemetry、Prometheus 和 Grafana。

当前状态：Phase 2 Domain + Database。

## 技术栈目标

- .NET 10
- C#
- ASP.NET Core Web API
- Blazor WebAssembly
- Entity Framework Core
- PostgreSQL
- xUnit
- Docker / Docker Compose
- GitHub Actions
- Terraform
- Azure Container Apps
- OpenTelemetry
- Prometheus
- Grafana

## Solution 结构

```text
src/
  DailyExpense.Api/
  DailyExpense.Blazor/
  DailyExpense.Application/
  DailyExpense.Domain/
  DailyExpense.Infrastructure/
  DailyExpense.Contracts/

tests/
  DailyExpense.UnitTests/
  DailyExpense.IntegrationTests/

ops/
  prometheus/
  grafana/
```

## 项目职责

```text
DailyExpense.Api
  REST API、Health Checks、后续接入 OpenTelemetry 和 API endpoints

DailyExpense.Blazor
  前端 UI，后续实现 Dashboard、Expenses、Categories、Budgets、Reports

DailyExpense.Domain
  领域实体、值对象、领域规则

DailyExpense.Application
  用例、应用服务、接口定义、业务编排

DailyExpense.Infrastructure
  EF Core、PostgreSQL、Repository、CSV、外部基础设施适配

DailyExpense.Contracts
  API 与 Blazor 共享的 request/response DTO
```

## 本地前置条件

当前项目目标框架是：

```text
net10.0
```

需要安装 .NET 10 SDK 后才能 restore/build/test。

检查 SDK：

```powershell
dotnet --list-sdks
```

## 常用命令

安装本仓库本地 .NET 工具：

```powershell
dotnet tool restore
```

Restore：

```powershell
dotnet restore DailyExpenseManager.sln
```

Build：

```powershell
dotnet build DailyExpenseManager.sln
```

Test：

```powershell
dotnet test DailyExpenseManager.sln
```

启动 PostgreSQL：

```powershell
docker compose up -d postgres
```

应用数据库 migration：

```powershell
dotnet dotnet-ef database update `
  --project src/DailyExpense.Infrastructure `
  --startup-project src/DailyExpense.Api
```

启动后端 API：

```powershell
dotnet run --project src/DailyExpense.Api --urls http://localhost:5000
```

访问：

```text
http://localhost:5000/
http://localhost:5000/health
http://localhost:5000/swagger
```

启动基础设施和应用容器：

```powershell
docker compose up --build
```

默认地址：

```text
API:        http://localhost:5000
Blazor:     http://localhost:5173
PostgreSQL: localhost:5432
Prometheus: http://localhost:9090
Grafana:    http://localhost:3000
```

Grafana 默认账号：

```text
admin / admin
```

## Blazor UI

当前 Blazor 已实现：

```text
基础应用布局
Dashboard 首页
API client 配置
Dashboard API 调用
加载状态
错误状态
空数据状态
```

访问前端：

```text
http://localhost:5173
```

Blazor API 地址配置：

```text
src/DailyExpense.Blazor/wwwroot/appsettings.json
```

## 当前 API

当前只提供基础健康检查：

```text
GET /
GET /health
```

Expense CRUD：

```text
GET    /api/v1/expenses
GET    /api/v1/expenses/{id}
POST   /api/v1/expenses
PUT    /api/v1/expenses/{id}
DELETE /api/v1/expenses/{id}
```

Category CRUD：

```text
GET    /api/v1/categories
GET    /api/v1/categories/{id}
POST   /api/v1/categories
PUT    /api/v1/categories/{id}
DELETE /api/v1/categories/{id}
```

分类删除规则：

```text
默认分类不能删除
已被支出引用的分类不能删除
分类名称必须唯一
```

Monthly Budget CRUD：

```text
GET    /api/v1/budgets?year=2026&month=8
GET    /api/v1/budgets/{id}
POST   /api/v1/budgets
PUT    /api/v1/budgets/{id}
DELETE /api/v1/budgets/{id}
```

预算规则：

```text
categoryId = null 表示整月总预算
categoryId = 分类 Id 表示分类预算
Year 必须在 2000 到 2100 之间
Month 必须在 1 到 12 之间
Amount 必须大于 0
同一个年月只能有一个总预算
同一个年月同一个分类只能有一个分类预算
分类预算的 categoryId 必须存在
```

预算返回值包含：

```text
amount
spentAmount
remainingAmount
usagePercentage
isOverBudget
```

创建总预算示例：

```powershell
$body = @{
  year = 2026
  month = 8
  amount = 1500
  categoryId = $null
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5000/api/v1/budgets" `
  -ContentType "application/json" `
  -Body $body
```

Summaries / Statistics / Dashboard：

```text
GET /api/v1/summaries/monthly?year=2026&month=8
GET /api/v1/summaries/yearly?year=2026

GET /api/v1/statistics/by-category?from=2026-08-01&to=2026-08-31
GET /api/v1/statistics/by-day?year=2026&month=8
GET /api/v1/statistics/trends?months=6

GET /api/v1/dashboard?year=2026&month=8
```

报表返回内容包括：

```text
月度总支出
年度总支出
分类支出占比
每日支出趋势
最近支出
总预算使用率
超支分类
```

## API Polish

当前 API 已启用：

```text
Swagger / OpenAPI
CORS for Blazor localhost origin
ProblemDetails error responses
Global exception handling
Status code problem responses
```

Swagger UI：

```text
http://localhost:5000/swagger
```

OpenAPI JSON：

```text
http://localhost:5000/swagger/v1/swagger.json
```

CORS 默认允许：

```text
http://localhost:5173
https://localhost:5173
```

错误响应使用 `application/problem+json`，并包含 `traceId`。

Expense list 支持查询参数：

```text
from
to
categoryId
minAmount
maxAmount
page
pageSize
sort
```

`sort` 支持：

```text
date_asc
date_desc
amount_asc
amount_desc
created_asc
created_desc
```

创建支出示例：

```powershell
$body = @{
  title = "Coffee"
  amount = 4.50
  expenseDate = "2026-08-07"
  categoryId = "11111111-1111-1111-1111-111111111111"
  paymentMethod = "Cash"
  description = "Morning coffee"
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5000/api/v1/expenses" `
  -ContentType "application/json" `
  -Body $body
```

## Phase 2 数据层

已添加领域实体：

```text
Category
Expense
MonthlyBudget
PaymentMethod
```

已添加 EF Core 基础设施：

```text
DailyExpenseDbContext
CategoryConfiguration
ExpenseConfiguration
MonthlyBudgetConfiguration
DailyExpenseDbContextFactory
DefaultCategories seed
InitialCreate migration
```

生成初始 migration 的命令：

```powershell
dotnet ef migrations add InitialCreate `
  --project src/DailyExpense.Infrastructure `
  --startup-project src/DailyExpense.Api `
  --output-dir Persistence/Migrations
```

应用数据库：

```powershell
dotnet ef database update `
  --project src/DailyExpense.Infrastructure `
  --startup-project src/DailyExpense.Api
```

注意：需要本机安装 .NET 10 SDK。本仓库已通过 `dotnet-tools.json` 固定本地 `dotnet-ef` 版本。
