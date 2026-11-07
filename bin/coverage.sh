#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet test tests/AudioCpp.Tests -v q --nologo /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=coverage/
reportgenerator -reports:tests/AudioCpp.Tests/coverage/coverage.net10.0.cobertura.xml -targetdir:tests/AudioCpp.Tests/coverage/report
echo "report: tests/AudioCpp.Tests/coverage/report/index.html"
