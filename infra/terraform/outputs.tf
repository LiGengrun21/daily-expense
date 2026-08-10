output "resource_group_name" {
  description = "Name of the Azure resource group."
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Azure region of the resource group."
  value       = azurerm_resource_group.main.location
}

output "log_analytics_workspace_id" {
  description = "Resource ID of the Log Analytics workspace."
  value       = azurerm_log_analytics_workspace.main.id
}

output "log_analytics_workspace_name" {
  description = "Name of the Log Analytics workspace."
  value       = azurerm_log_analytics_workspace.main.name
}

output "container_registry_name" {
  description = "Name of the Azure Container Registry."
  value       = azurerm_container_registry.main.name
}

output "container_registry_login_server" {
  description = "Login server of the Azure Container Registry."
  value       = azurerm_container_registry.main.login_server
}

output "container_apps_environment_id" {
  description = "Resource ID of the Azure Container Apps Environment."
  value       = azurerm_container_app_environment.main.id
}

output "container_apps_environment_name" {
  description = "Name of the Azure Container Apps Environment."
  value       = azurerm_container_app_environment.main.name
}

output "container_apps_identity_id" {
  description = "Resource ID of the user assigned managed identity used by Container Apps."
  value       = azurerm_user_assigned_identity.container_apps.id
}

output "container_apps_identity_client_id" {
  description = "Client ID of the user assigned managed identity used by Container Apps."
  value       = azurerm_user_assigned_identity.container_apps.client_id
}

output "container_apps_identity_principal_id" {
  description = "Principal ID of the user assigned managed identity used by Container Apps."
  value       = azurerm_user_assigned_identity.container_apps.principal_id
}

output "api_container_app_name" {
  description = "Name of the API Container App."
  value       = try(azurerm_container_app.api[0].name, null)
}

output "api_container_app_fqdn" {
  description = "FQDN of the API Container App ingress."
  value       = try(azurerm_container_app.api[0].ingress[0].fqdn, null)
}

output "api_container_app_url" {
  description = "Public HTTPS URL of the API Container App."
  value       = var.deploy_container_apps ? local.api_public_url : null
}

output "blazor_container_app_name" {
  description = "Name of the Blazor Container App."
  value       = try(azurerm_container_app.blazor[0].name, null)
}

output "blazor_container_app_fqdn" {
  description = "FQDN of the Blazor Container App ingress."
  value       = try(azurerm_container_app.blazor[0].ingress[0].fqdn, null)
}

output "blazor_container_app_url" {
  description = "Public HTTPS URL of the Blazor Container App."
  value       = var.deploy_container_apps ? local.blazor_public_url : null
}

output "postgres_server_name" {
  description = "Name of the Azure PostgreSQL Flexible Server."
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgres_server_fqdn" {
  description = "FQDN of the Azure PostgreSQL Flexible Server."
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgres_database_name" {
  description = "Name of the Azure PostgreSQL database."
  value       = azurerm_postgresql_flexible_server_database.main.name
}
