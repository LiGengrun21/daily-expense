#!/bin/sh
set -eu

api_base_url="${API_BASE_URL:-http://localhost:5000}"

cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "Api": {
    "BaseUrl": "${api_base_url}"
  }
}
EOF
