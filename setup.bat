@echo off
setlocal
where dotnet >nul 2>nul
if %errorlevel%==0 (
  dotnet build "%~dp0csharp\MedBook.Booking.Tests\MedBook.Booking.Tests.csproj" --nologo
  echo Setup complete. Run: dotnet run --project csharp/MedBook.Booking.Tests
  exit /b %errorlevel%
)
where py >nul 2>nul
if %errorlevel%==0 (
  py -3 -m compileall -q "%~dp0python"
  echo Setup complete. Run: py -3 -m unittest discover -s python/tests -v
  exit /b %errorlevel%
)
echo Neither .NET 8 nor Python 3 was found. Tell the interviewer; this is not scored.
exit /b 1
