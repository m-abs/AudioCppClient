#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
rm -rf _site   # docfx build does not clean _site; stale md pages leave ghost output
docfx          # bare docfx = metadata + build; plain `docfx build` skips metadata
docfx serve _site -p 8090
