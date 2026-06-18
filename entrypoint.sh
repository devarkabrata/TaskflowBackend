#!/bin/sh
# Render injects PORT at runtime. Fall back to 8080 for local testing.
exec dotnet TaskFlowBackend.dll --urls "http://+:${PORT:-8080}"
