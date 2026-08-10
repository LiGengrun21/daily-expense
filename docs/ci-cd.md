# CI/CD

This document describes the CI/CD workflow for the Daily Expense Manager
project.

The workflow file is:

```text
.github/workflows/ci-cd.yml
```

## Goals

The CI/CD pipeline is designed to:

- Validate code quality before deployment.
- Run automated tests.
- Verify the app can start locally with Docker Compose.
- Build API and Blazor container images.
- Push images to Azure Container Registry.
- Deploy the latest images to Azure Container Apps.
- Verify the deployed Azure services are reachable.

Terraform is currently used to create and maintain Azure infrastructure
manually. The CI/CD pipeline does not run `terraform plan` or `terraform apply`
yet.

## Triggers

The workflow runs on:

```text
push -> master
push -> develop
pull_request -> master
pull_request -> develop
manual workflow_dispatch
```

Deployment to Azure only runs for:

```text
push -> master
manual workflow_dispatch
```

Pull requests and pushes to `develop` run validation only and do not deploy.

## Concurrency

The workflow uses concurrency control:

```yaml
concurrency:
  group: ci-cd-${{ github.ref }}
  cancel-in-progress: true
```

Purpose:

- Only one CI/CD run per branch should be active at the same time.
- If several commits are pushed quickly, older in-progress runs are cancelled.
- The newest commit is the one that gets validated and deployed.

This avoids an older deployment finishing after a newer deployment.

## Pipeline flow

```text
dotnet
  -> docker-smoke
    -> deploy-azure
```

The `deploy-azure` job depends on both validation jobs. If build, tests, or the
Docker Compose smoke test fail, deployment will not run.

## Job: dotnet

Purpose:

```text
Validate the .NET solution.
```

Steps:

| Step | Purpose |
| --- | --- |
| Checkout | Downloads the repository source code into the GitHub runner. |
| Setup .NET | Installs the .NET 10 SDK. |
| Restore | Restores NuGet packages for the solution. |
| Format check | Verifies formatting without changing files. |
| Build | Builds the full solution in Release mode. |
| Test | Runs xUnit tests. |
| Upload test results | Uploads `.trx` test results as workflow artifacts. |

## Job: docker-smoke

Purpose:

```text
Verify that the application can be built and started with Docker Compose.
```

This catches problems that normal `dotnet build` cannot catch, such as:

- Dockerfile errors.
- Missing runtime files.
- Wrong container ports.
- Basic startup failures.
- API health check failures.

Steps:

| Step | Purpose |
| --- | --- |
| Checkout | Downloads the repository source code into the GitHub runner. |
| Build Docker images | Builds the local API and Blazor images. |
| Start application stack | Starts PostgreSQL, API, and Blazor with Docker Compose. |
| Wait for API health | Calls `http://localhost:5000/health` until the API is ready. |
| Check Blazor host | Calls `http://localhost:5173/` to verify the frontend host responds. |
| Docker Compose status | Prints container status for diagnostics. |
| Docker Compose logs | Prints logs when a failure occurs. |
| Stop application stack | Cleans up containers and volumes. |

## Job: deploy-azure

Purpose:

```text
Build, publish, deploy, and verify the Azure-hosted application.
```

This job only runs for `master` pushes or manual dispatches:

```yaml
if: github.event_name == 'workflow_dispatch' || (github.event_name == 'push' && github.ref == 'refs/heads/master')
```

The job is attached to the GitHub Environment:

```yaml
environment: dev
```

Purpose:

- Makes the target deployment environment visible in GitHub Actions.
- Allows dev-specific environment variables and secrets later.
- Allows protection rules or manual approvals to be added later without
  redesigning the workflow.

Steps:

| Step | Purpose |
| --- | --- |
| Checkout | Downloads the repository source code into the GitHub runner. |
| Azure login | Logs into Azure using GitHub OIDC. |
| Resolve image metadata | Calculates the image tag and reads the ACR login server. |
| Docker login to ACR | Authenticates Docker against Azure Container Registry. |
| Build API image | Builds the ASP.NET Core Web API container image. |
| Build Blazor image | Builds the Blazor static frontend nginx image. |
| Push API image | Pushes the API image to ACR using commit SHA and `latest` tags. |
| Push Blazor image | Pushes the Blazor image to ACR using commit SHA and `latest` tags. |
| Enable Azure CLI dynamic extensions | Allows Azure CLI to install the Container Apps extension if needed. |
| Deploy API Container App | Updates the API Container App to use the new API image tag. |
| Deploy Blazor Container App | Updates the Blazor Container App to use the new Blazor image tag. |
| Resolve deployed URLs | Reads the public API and Blazor URLs from Azure Container Apps. |
| Smoke test deployed API | Calls the deployed API `/health` endpoint and prints logs if it fails. |
| Smoke test deployed Blazor | Calls the deployed Blazor root page and `appsettings.json`. |
| Summary | Writes published image names and deployed URLs to the workflow summary. |

## Image tags

Each deployment publishes two tags for each image:

```text
<commit-sha>
latest
```

Example:

```text
acrdailyexpensedev.azurecr.io/daily-expense-api:abc1234
acrdailyexpensedev.azurecr.io/daily-expense-api:latest
```

The Container Apps are updated to the commit SHA tag, not `latest`. This makes
the deployed version explicit and easier to trace.

## Required GitHub secrets

These values are configured in:

```text
GitHub repository -> Settings -> Secrets and variables -> Actions -> Secrets
```

| Secret | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID` | Client ID of the Microsoft Entra application used by GitHub Actions. |
| `AZURE_TENANT_ID` | Tenant ID where the Microsoft Entra application and service principal exist. |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription used for deployment. |

## Required GitHub variables

These values are configured in:

```text
GitHub repository -> Settings -> Secrets and variables -> Actions -> Variables
```

| Variable | Purpose | Dev value |
| --- | --- | --- |
| `ACR_NAME` | Azure Container Registry name. | `acrdailyexpensedev` |
| `AZURE_RESOURCE_GROUP` | Azure resource group name. | `rg-daily-expense-dev` |
| `API_CONTAINER_APP_NAME` | API Container App name. | `ca-daily-expense-dev-api` |
| `BLAZOR_CONTAINER_APP_NAME` | Blazor Container App name. | `ca-daily-expense-dev-blazor` |

The workflow has default dev values for these variables, but configuring them in
GitHub keeps the deployment environment explicit.

## Azure identity requirements

GitHub Actions uses OIDC to log into Azure. The Azure identity must have:

- Microsoft Entra App Registration.
- Matching Service Principal / Enterprise Application.
- Federated Credential for the GitHub `dev` environment.
- RBAC permission to update resources in `rg-daily-expense-dev`.

For the MVP, resource group level `Contributor` is enough.

Because the `deploy-azure` job uses:

```yaml
environment: dev
```

the Azure federated credential subject must be environment-based:

```text
repo:<github-owner>/<github-repo>:environment:dev
```

A branch-based subject such as this will not match the deployment job:

```text
repo:<github-owner>/<github-repo>:ref:refs/heads/master
```

## Current limitations

The current CI/CD pipeline does not yet:

- Run EF Core migrations against Azure PostgreSQL.
- Run Terraform from GitHub Actions.
- Use Terraform remote state in CI/CD.
- Use Azure Key Vault for application secrets.
- Implement production approvals or rollback automation.

For the current MVP stage, Terraform is run manually and CI/CD focuses on
application delivery.
