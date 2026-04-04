#!/usr/bin/env bash
# Pulls the default model used by DeviceManagement (LlmDescription:Model, usually llama3.2).
# Prerequisites: Ollama installed from https://ollama.com and `ollama serve` running (or use Docker compose).

set -euo pipefail

MODEL="${OLLAMA_MODEL:-llama3.2}"

if ! command -v ollama >/dev/null 2>&1; then
  echo "Ollama is not installed or not on PATH."
  echo "Install from https://ollama.com then run:"
  echo "  ollama serve"
  echo "  $0"
  echo ""
  echo "Or use Docker: docker compose -f docker-compose.ollama.yml up -d"
  exit 1
fi

echo "Pulling model: $MODEL (this may take a few minutes the first time)..."
ollama pull "$MODEL"
echo "Done. Ensure the daemon is running (ollama serve) while you use the API."
