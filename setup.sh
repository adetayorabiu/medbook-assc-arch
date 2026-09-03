#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"

if command -v dotnet >/dev/null 2>&1; then
  echo "Primary C# environment detected: $(dotnet --version)"
  dotnet build "$ROOT/csharp/MedBook.Booking.Tests/MedBook.Booking.Tests.csproj" --nologo
  echo "Setup complete. Run: dotnet run --project csharp/MedBook.Booking.Tests"
elif command -v python3 >/dev/null 2>&1; then
  echo "Python fallback detected: $(python3 --version)"
  python3 -m compileall -q "$ROOT/python"
  echo "Setup complete. Run: python3 -m unittest discover -s python/tests -v"
else
  echo "Neither .NET 8 nor Python 3 was found. Tell the interviewer; this is not scored." >&2
  exit 1
fi
