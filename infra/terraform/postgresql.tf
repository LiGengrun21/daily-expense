locals {
  postgres_server_name = coalesce(
    var.postgres_server_name,
    substr("pg-${local.resource_prefix}", 0, 63)
  )

  generated_api_connection_string = join(";", [
    "Host=${azurerm_postgresql_flexible_server.main.fqdn}",
    "Port=5432",
    "Database=${azurerm_postgresql_flexible_server_database.main.name}",
    "Username=${var.postgres_admin_username}",
    "Password=${var.postgres_admin_password}",
    "SSL Mode=Require",
    "Trust Server Certificate=true"
  ])

  api_connection_string = coalesce(var.api_connection_string, local.generated_api_connection_string)
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                          = local.postgres_server_name
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = var.postgres_version
  administrator_login           = var.postgres_admin_username
  administrator_password        = var.postgres_admin_password
  sku_name                      = var.postgres_sku_name
  zone                          = var.postgres_zone
  storage_mb                    = var.postgres_storage_mb
  backup_retention_days         = var.postgres_backup_retention_days
  public_network_access_enabled = var.postgres_public_network_access_enabled
  tags                          = local.common_tags

  lifecycle {
    ignore_changes = [
      zone
    ]
  }
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.postgres_database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "azure_services" {
  count = var.postgres_allow_azure_services ? 1 : 0

  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
