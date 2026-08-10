locals {
  api_container_app_name    = "ca-${local.resource_prefix}-api"
  blazor_container_app_name = "ca-${local.resource_prefix}-blazor"
  api_image                 = "${azurerm_container_registry.main.login_server}/${var.api_image_name}:${var.container_image_tag}"
  blazor_image              = "${azurerm_container_registry.main.login_server}/${var.blazor_image_name}:${var.container_image_tag}"
  api_public_url            = "https://${local.api_container_app_name}.${azurerm_container_app_environment.main.default_domain}"
  blazor_public_url         = "https://${local.blazor_container_app_name}.${azurerm_container_app_environment.main.default_domain}"
}

resource "azurerm_container_app" "api" {
  count = var.deploy_container_apps ? 1 : 0

  name                         = local.api_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.common_tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  secret {
    name  = "default-connection-string"
    value = local.api_connection_string
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.api_min_replicas
    max_replicas = var.api_max_replicas

    container {
      name   = "api"
      image  = local.api_image
      cpu    = 0.5
      memory = "1.0Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.api_environment_name
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      }

      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "default-connection-string"
      }

      env {
        name  = "Cors__AllowedOrigins__0"
        value = local.blazor_public_url
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.container_apps_acr_pull
  ]
}

resource "azurerm_container_app" "blazor" {
  count = var.deploy_container_apps ? 1 : 0

  name                         = local.blazor_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  tags                         = local.common_tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  ingress {
    external_enabled = true
    target_port      = 80
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.blazor_min_replicas
    max_replicas = var.blazor_max_replicas

    container {
      name   = "blazor"
      image  = local.blazor_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "API_BASE_URL"
        value = local.api_public_url
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.container_apps_acr_pull
  ]
}
