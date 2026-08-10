variable "project_name" {
  description = "Project name used in Azure resource names."
  type        = string
  default     = "daily-expense"

  validation {
    condition     = can(regex("^[a-z0-9-]{2,30}$", var.project_name))
    error_message = "project_name must be 2-30 characters and contain only lowercase letters, numbers, and hyphens."
  }
}

variable "environment" {
  description = "Deployment environment name."
  type        = string
  default     = "dev"

  validation {
    condition     = can(regex("^[a-z0-9-]{2,12}$", var.environment))
    error_message = "environment must be 2-12 characters and contain only lowercase letters, numbers, and hyphens."
  }
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "northeurope"
}

variable "subscription_id" {
  description = "Azure subscription ID used by the AzureRM provider."
  type        = string
  sensitive   = true

  validation {
    condition     = can(regex("^[0-9a-fA-F-]{36}$", var.subscription_id))
    error_message = "subscription_id must be a valid Azure subscription GUID."
  }
}

variable "container_registry_name" {
  description = "Globally unique Azure Container Registry name. Use only lowercase letters and numbers."
  type        = string
  default     = null

  validation {
    condition = (
      var.container_registry_name == null
      || can(regex("^[a-z0-9]{5,50}$", var.container_registry_name))
    )
    error_message = "container_registry_name must be 5-50 characters and contain only lowercase letters and numbers."
  }
}

variable "log_retention_days" {
  description = "Log Analytics retention period in days."
  type        = number
  default     = 30

  validation {
    condition     = var.log_retention_days >= 30 && var.log_retention_days <= 730
    error_message = "log_retention_days must be between 30 and 730."
  }
}

variable "tags" {
  description = "Common tags applied to Azure resources."
  type        = map(string)
  default     = {}
}

variable "container_image_tag" {
  description = "Container image tag used by the API and Blazor Container Apps."
  type        = string
  default     = "latest"
}

variable "deploy_container_apps" {
  description = "Whether to deploy the API and Blazor Container Apps. Set to false for the first foundation-only apply, then true after images are pushed to ACR."
  type        = bool
  default     = false
}

variable "api_image_name" {
  description = "API container image repository name in ACR."
  type        = string
  default     = "daily-expense-api"
}

variable "blazor_image_name" {
  description = "Blazor container image repository name in ACR."
  type        = string
  default     = "daily-expense-blazor"
}

variable "api_connection_string" {
  description = "Optional PostgreSQL connection string override for the API Container App. When null, Terraform builds it from the Azure PostgreSQL resource."
  type        = string
  sensitive   = true
  default     = null
}

variable "api_environment_name" {
  description = "ASPNETCORE_ENVIRONMENT value for the API Container App."
  type        = string
  default     = "Production"
}

variable "api_min_replicas" {
  description = "Minimum API Container App replicas."
  type        = number
  default     = 0
}

variable "api_max_replicas" {
  description = "Maximum API Container App replicas."
  type        = number
  default     = 1
}

variable "blazor_min_replicas" {
  description = "Minimum Blazor Container App replicas."
  type        = number
  default     = 0
}

variable "blazor_max_replicas" {
  description = "Maximum Blazor Container App replicas."
  type        = number
  default     = 1
}

variable "postgres_server_name" {
  description = "Optional Azure PostgreSQL Flexible Server name. Must be globally unique."
  type        = string
  default     = null

  validation {
    condition = (
      var.postgres_server_name == null
      || can(regex("^[a-z0-9-]{3,63}$", var.postgres_server_name))
    )
    error_message = "postgres_server_name must be 3-63 characters and contain only lowercase letters, numbers, and hyphens."
  }
}

variable "postgres_database_name" {
  description = "PostgreSQL database name."
  type        = string
  default     = "daily_expense"

  validation {
    condition     = can(regex("^[a-zA-Z0-9_]{1,63}$", var.postgres_database_name))
    error_message = "postgres_database_name must be 1-63 characters and contain only letters, numbers, and underscores."
  }
}

variable "postgres_admin_username" {
  description = "PostgreSQL administrator username."
  type        = string
  default     = "dailyexpenseadmin"

  validation {
    condition     = can(regex("^[a-zA-Z][a-zA-Z0-9_]{2,62}$", var.postgres_admin_username))
    error_message = "postgres_admin_username must start with a letter and be 3-63 characters."
  }
}

variable "postgres_admin_password" {
  description = "PostgreSQL administrator password."
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.postgres_admin_password) >= 12
    error_message = "postgres_admin_password must be at least 12 characters."
  }
}

variable "postgres_version" {
  description = "PostgreSQL major version."
  type        = string
  default     = "16"
}

variable "postgres_sku_name" {
  description = "Azure PostgreSQL Flexible Server SKU."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_zone" {
  description = "Optional availability zone for Azure PostgreSQL Flexible Server. Leave null to let Azure choose."
  type        = string
  default     = null
}

variable "postgres_storage_mb" {
  description = "Azure PostgreSQL storage size in MB."
  type        = number
  default     = 32768
}

variable "postgres_backup_retention_days" {
  description = "PostgreSQL backup retention period in days."
  type        = number
  default     = 7

  validation {
    condition     = var.postgres_backup_retention_days >= 7 && var.postgres_backup_retention_days <= 35
    error_message = "postgres_backup_retention_days must be between 7 and 35."
  }
}

variable "postgres_public_network_access_enabled" {
  description = "Whether public network access is enabled for Azure PostgreSQL."
  type        = bool
  default     = true
}

variable "postgres_allow_azure_services" {
  description = "Whether Azure services can access Azure PostgreSQL through a firewall rule."
  type        = bool
  default     = true
}
