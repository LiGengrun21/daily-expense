# Terraform

This folder contains the Azure infrastructure code for the Daily Expense
Manager application.

## Current resources

The current Terraform configuration creates these Azure resources:

- Resource Group
- Log Analytics Workspace
- Azure Container Registry
- Azure Container Apps Environment
- User Assigned Managed Identity
- AcrPull role assignment for the managed identity
- API Container App
- Blazor Container App
- Azure PostgreSQL Flexible Server
- PostgreSQL database

Later steps can add:

- Key Vault

## Local usage

Prerequisites:

- Azure CLI
- Terraform
- Azure subscription access

Commands:

```bash
az login
az account set --subscription "<subscription-id>"

cd infra/terraform
cp terraform.tfvars.example terraform.tfvars
```

Update `terraform.tfvars` with your real `subscription_id`. If the default ACR
name is already taken, also set `container_registry_name`.

Set `postgres_admin_password` to a real strong password. Terraform builds the
API connection string from the PostgreSQL resource by default. You can still set
`api_connection_string` manually if you need to override it.
Leave `postgres_zone = null` unless you explicitly need a specific availability
zone.

The images referenced by `api_image_name`, `blazor_image_name`, and
`container_image_tag` must exist in ACR before applying the Container Apps.
When `deploy_container_apps = true`, Terraform configures:

- API `Cors__AllowedOrigins__0` from the Blazor Container App URL
- Blazor `API_BASE_URL` from the API Container App URL

The Blazor nginx image generates `wwwroot/appsettings.json` at container startup
from `API_BASE_URL`. If `API_BASE_URL` is not set, it falls back to
`http://localhost:5000` for local Docker Compose usage.

For local Azure + Terraform practice, use a two-phase deployment:

1. Keep `deploy_container_apps = false` and apply the foundation resources.
2. Build and push the API and Blazor images to ACR.
3. Set `deploy_container_apps = true` and apply again to create the Container Apps.

Then run:

```bash
terraform init
terraform fmt
terraform validate
terraform plan
terraform apply
```

After the first apply, get the ACR login server:

```bash
terraform output container_registry_login_server
az acr login --name "<acr-name>"
```

Build and push images:

```bash
docker build -f ../../src/DailyExpense.Api/Dockerfile -t <acr-login-server>/daily-expense-api:latest ../..
docker build -f ../../src/DailyExpense.Blazor/Dockerfile -t <acr-login-server>/daily-expense-blazor:latest ../..

docker push <acr-login-server>/daily-expense-api:latest
docker push <acr-login-server>/daily-expense-blazor:latest
```

Then set:

```hcl
deploy_container_apps = true
```

Run `terraform plan` and `terraform apply` again.

Do not commit `terraform.tfvars`, `.terraform`, or `terraform.tfstate`.

## GitHub Actions configuration

The local `terraform.tfvars` file is for local Terraform runs only. Do not commit
it and do not depend on it from CI/CD. GitHub Actions should receive deployment
configuration from repository secrets and variables.

Open the GitHub repository, then go to:

```text
Settings -> Secrets and variables -> Actions
```

Add sensitive values under `Secrets` and non-sensitive environment names under
`Variables`.

### Required secrets

These secrets are required by the current image publish and Container Apps
deployment workflow.

| Secret | Purpose | How to get the value |
| --- | --- | --- |
| `AZURE_CLIENT_ID` | Client ID used by `azure/login` for GitHub Actions OIDC authentication. | Create or reuse a Microsoft Entra application for GitHub Actions. Run `az ad app create --display-name daily-expense-github-actions --query appId --output tsv`. |
| `AZURE_TENANT_ID` | Azure tenant ID used by `azure/login`. | Run `az account show --query tenantId --output tsv`. |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription used by `azure/login`. | Run `az account show --query id --output tsv`. |

Create a service principal for the app if it does not already exist:

```bash
az ad sp create --id "<AZURE_CLIENT_ID>"
```

Grant the app permission to update the dev resources. For the MVP, resource
group level `Contributor` is enough:

```bash
subscription_id="$(az account show --query id --output tsv)"

az role assignment create \
  --assignee "<AZURE_CLIENT_ID>" \
  --role Contributor \
  --scope "/subscriptions/${subscription_id}/resourceGroups/rg-daily-expense-dev"
```

Configure the federated credential for the GitHub `master` branch. Replace
`<github-owner>` and `<github-repo>` with the real repository owner and name.

```json
{
  "name": "daily-expense-master",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<github-owner>/<github-repo>:ref:refs/heads/master",
  "description": "GitHub Actions master branch deployment",
  "audiences": [
    "api://AzureADTokenExchange"
  ]
}
```

Save that JSON to a temporary file, then run:

```bash
az ad app federated-credential create \
  --id "<AZURE_CLIENT_ID>" \
  --parameters credential.json
```

### Required variables

These repository variables are used by the current image publish and Container
Apps deployment workflow. They are not secrets.

| Variable | Purpose | Current dev value / source |
| --- | --- | --- |
| `ACR_NAME` | Azure Container Registry resource name used for Docker login and image push. | `acrdailyexpensedev`, or run `terraform output container_registry_name`. |
| `AZURE_RESOURCE_GROUP` | Resource group containing the Container Apps. | `rg-daily-expense-dev`, or run `terraform output resource_group_name`. |
| `API_CONTAINER_APP_NAME` | API Container App name to update after pushing an image. | `ca-daily-expense-dev-api`, or run `terraform output api_container_app_name`. |
| `BLAZOR_CONTAINER_APP_NAME` | Blazor Container App name to update after pushing an image. | `ca-daily-expense-dev-blazor`, or run `terraform output blazor_container_app_name`. |

The workflow has dev defaults for these variables, but setting them in GitHub is
clearer and makes later environment changes safer.

### Terraform CI/CD variables

If GitHub Actions later runs `terraform plan` or `terraform apply`, move the
values from `terraform.tfvars` into GitHub secrets and variables using
Terraform's `TF_VAR_` environment variable convention.

Terraform variable:

```hcl
project_name = "daily-expense"
```

GitHub Actions environment variable:

```text
TF_VAR_project_name=daily-expense
```

Sensitive Terraform values should be GitHub secrets:

| Secret | Purpose | How to get the value |
| --- | --- | --- |
| `TF_VAR_subscription_id` | Passed to the Terraform `subscription_id` variable. Usually the same value as `AZURE_SUBSCRIPTION_ID`. | Run `az account show --query id --output tsv`. |
| `TF_VAR_postgres_admin_password` | Passed to the Terraform `postgres_admin_password` variable. | Use the same strong password used for local `terraform.tfvars`, or rotate it and update Azure PostgreSQL accordingly. |

Non-sensitive Terraform values can be GitHub variables:

| Variable | Example value |
| --- | --- |
| `TF_VAR_project_name` | `daily-expense` |
| `TF_VAR_environment` | `dev` |
| `TF_VAR_location` | `northeurope` |
| `TF_VAR_log_retention_days` | `30` |
| `TF_VAR_deploy_container_apps` | `true` |
| `TF_VAR_container_image_tag` | `latest` |
| `TF_VAR_api_image_name` | `daily-expense-api` |
| `TF_VAR_blazor_image_name` | `daily-expense-blazor` |
| `TF_VAR_api_environment_name` | `Production` |
| `TF_VAR_api_min_replicas` | `0` |
| `TF_VAR_api_max_replicas` | `1` |
| `TF_VAR_blazor_min_replicas` | `0` |
| `TF_VAR_blazor_max_replicas` | `1` |
| `TF_VAR_postgres_database_name` | `daily_expense` |
| `TF_VAR_postgres_admin_username` | `dailyexpenseadmin` |
| `TF_VAR_postgres_version` | `16` |
| `TF_VAR_postgres_sku_name` | `B_Standard_B1ms` |
| `TF_VAR_postgres_storage_mb` | `32768` |
| `TF_VAR_postgres_backup_retention_days` | `7` |
| `TF_VAR_postgres_public_network_access_enabled` | `true` |
| `TF_VAR_postgres_allow_azure_services` | `true` |

Most of these already have defaults in `variables.tf`. The minimum required
Terraform values for CI/CD are `TF_VAR_subscription_id` and
`TF_VAR_postgres_admin_password`.

### Future migration secret

If a later deployment workflow runs EF Core migrations against Azure PostgreSQL,
add this extra secret:

| Secret | Purpose | How to get the value |
| --- | --- | --- |
| `AZURE_POSTGRES_CONNECTION_STRING` | Connection string used by `dotnet ef database update`. | Build it from the Terraform PostgreSQL output and the admin password: `Host=<postgres-fqdn>;Port=5432;Database=daily_expense;Username=dailyexpenseadmin;Password=<password>;SSL Mode=Require;Trust Server Certificate=true`. |
