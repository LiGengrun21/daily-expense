#!/bin/sh
set -eu

api_base_url="${API_BASE_URL:-http://localhost:5000}"
entra_tenant_id="${ENTRA_TENANT_ID:-}"
entra_spa_client_id="${ENTRA_SPA_CLIENT_ID:-}"

# These identifiers are public GUIDs; reject characters that could corrupt JSON.
case "${entra_tenant_id}${entra_spa_client_id}" in
  *[!a-fA-F0-9-]*) echo "Entra IDs must contain only GUID characters." >&2; exit 1 ;;
esac

cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "AzureAd": {
    "TenantId": "${entra_tenant_id}",
    "ClientId": "${entra_spa_client_id}"
  },
  "Api": {
    "BaseUrl": "${api_base_url}"
  }
}
EOF
