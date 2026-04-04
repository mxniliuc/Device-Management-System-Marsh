#!/usr/bin/env bash
# Seeds the API with diverse devices so you can try GET /api/devices/search?q=...
#
# Prerequisites: API running (e.g. dotnet run on http://localhost:5084), curl, jq.
# Optional env:
#   BASE_URL       — API root (default: http://localhost:5084)
#   SEED_EMAIL     — account email (default: seed-<timestamp>@search-demo.local)
#   SEED_PASSWORD  — min 8 chars (default: SeedDemo123!)
#
# Usage:
#   chmod +x scripts/seed-search-demo-devices.sh
#   ./scripts/seed-search-demo-devices.sh

set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5084}"
BASE_URL="${BASE_URL%/}"
PASSWORD="${SEED_PASSWORD:-SeedDemo123!}"
EMAIL="${SEED_EMAIL:-seed-$(date +%s)@search-demo.local}"

if ! command -v jq >/dev/null 2>&1; then
  echo "This script requires jq (https://jqlang.org/)." >&2
  exit 1
fi

register_json() {
  cat <<EOF
{
  "email": "$EMAIL",
  "password": "$PASSWORD",
  "confirmPassword": "$PASSWORD",
  "name": "Search Demo User",
  "role": "User",
  "location": "Lab"
}
EOF
}

curl_json_code() {
  # Writes body to $1, prints HTTP status code to stdout.
  local out="$1"
  shift
  curl -sS -o "$out" -w "%{http_code}" "$@"
}

TMP=$(mktemp)
trap 'rm -f "$TMP"' EXIT

echo "Registering $EMAIL ..."
REG_CODE=$(curl_json_code "$TMP" -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "$(register_json)")
REG_BODY=$(cat "$TMP")

if [[ "$REG_CODE" != "200" ]]; then
  echo "Register returned HTTP $REG_CODE. Trying login with same email/password ..."
  REG_CODE=$(curl_json_code "$TMP" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "$(jq -n --arg email "$EMAIL" --arg password "$PASSWORD" '{email:$email,password:$password}')")
  REG_BODY=$(cat "$TMP")
  if [[ "$REG_CODE" != "200" ]]; then
    echo "$REG_BODY" | jq . 2>/dev/null || echo "$REG_BODY"
    echo "Login also failed. Fix credentials or use a fresh SEED_EMAIL." >&2
    exit 1
  fi
fi

TOKEN=$(echo "$REG_BODY" | jq -r '.token // empty')
if [[ -z "$TOKEN" || "$TOKEN" == "null" ]]; then
  echo "No token in response:" >&2
  echo "$REG_BODY" >&2
  exit 1
fi

post_device() {
  local name="$1" manufacturer="$2" type="$3" os="$4" osv="$5" processor="$6" ram="$7" desc="$8" loc="$9"
  curl -sS -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/devices" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "$(jq -n \
      --arg name "$name" \
      --arg manufacturer "$manufacturer" \
      --arg type "$type" \
      --arg os "$os" \
      --arg osVersion "$osv" \
      --arg processor "$processor" \
      --argjson ramGb "$ram" \
      --arg description "$desc" \
      --arg location "$loc" \
      '{name:$name,manufacturer:$manufacturer,type:$type,os:$os,osVersion:$osVersion,processor:$processor,ramGb:$ramGb,description:$description,location:$location,assignedToUserId:null}')"
}

echo "Creating sample devices ..."
ok=0
fail=0

create() {
  local code
  code=$(post_device "$@")
  if [[ "$code" == "201" ]]; then
    ok=$((ok + 1))
  else
    echo "  WARN: POST device failed HTTP $code ($1)" >&2
    fail=$((fail + 1))
  fi
}

create "iPhone 17 Pro" "Apple" "Phone" "iOS" "26" "A19 Pro" 12 "Apple flagship phone" "NYC"
create "Pixel 9 Pro" "Google" "Phone" "Android" "15" "Tensor G4" 12 "Google phone" "SF"
create "Galaxy S24 Ultra" "Samsung" "Phone" "Android" "14" "Snapdragon 8 Gen 3" 12 "Samsung flagship" "Austin"
create "Surface Pro 11" "Microsoft" "Tablet" "Windows" "11" "Snapdragon X Elite" 16 "2-in-1 tablet" "Seattle"
create "iPad Pro 13" "Apple" "Tablet" "iPadOS" "18" "M4" 16 "Large tablet" "London"
create "Galaxy Tab S9" "Samsung" "Tablet" "Android" "14" "Snapdragon 8 Gen 2" 8 "Android tablet" "Berlin"
create "Kindle Paperwhite" "Amazon" "Tablet" "Kindle" "5" "MediaTek MT8113" 2 "E-reader" "Remote"
create "Fairphone 5" "Fairphone" "Phone" "Android" "13" "QCM6490" 8 "Repairable phone" "Amsterdam"
create "Nothing Phone (2)" "Nothing" "Phone" "Android" "14" "Snapdragon 8+ Gen 1" 12 "Transparent design" "Paris"

echo ""
echo "Done. Created $ok devices ($fail failed)."
echo "Account: $EMAIL (password: from SEED_PASSWORD or default SeedDemo123!)"
echo ""
echo "Try search examples (set TOKEN from the login/register response):"
echo "  curl -sS \"$BASE_URL/api/devices/search?q=apple+tensor\" -H \"Authorization: Bearer \$TOKEN\" | jq ."
echo "  curl -sS \"$BASE_URL/api/devices/search?q=16+snapdragon\" -H \"Authorization: Bearer \$TOKEN\" | jq ."
echo "  curl -sS \"$BASE_URL/api/devices/search?q=samsung+12\" -H \"Authorization: Bearer \$TOKEN\" | jq ."
