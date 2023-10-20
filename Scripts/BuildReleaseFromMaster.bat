@echo off

:: Check if dotnet is available
dotnet --version > nul 2>&1
if errorlevel 1 (
    echo Error: dotnet command not found. Please install .NET Core SDK.
    exit /b 1
)

echo Cloning repository...
set REPO_URL=https://github.com/FabiChan99/LavaSharp.git
set COMMIT_HASH=

git clone %REPO_URL%
cd LavaSharp

for /f "tokens=*" %%a in ('git rev-parse HEAD') do set COMMIT_HASH=%%a

echo Building for Windows...
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=false

echo Building for Linux...
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=false

:: Return to the original directory
cd ..

:: Create the release directory for Windows
mkdir LavaSharp-Windows-%COMMIT_HASH%
cd LavaSharp-Windows-%COMMIT_HASH%

:: Copy the Windows build
echo Copying Windows build...
copy ..\LavaSharp\bin\Release\net7.0\win-x64\publish\LavaSharp.exe

:: Copy the Example Config
echo Copying Example Config...
copy ..\LavaSharp\example.config.json

:: Create the release archive for Windows
cd ..
powershell Compress-Archive -Path "./LavaSharp-Windows-%COMMIT_HASH%" -DestinationPath "./LavaSharp-Windows-x86_64-%COMMIT_HASH%.zip"

:: Create the release directory for Linux
mkdir LavaSharp-Linux-%COMMIT_HASH%
cd LavaSharp-Linux-%COMMIT_HASH%

:: Copy the Linux build
echo Copying Linux build...
copy ..\LavaSharp\bin\Release\net7.0\linux-x64\publish\LavaSharp

:: Copy the Example Config
echo Copying Example Config...
copy ..\LavaSharp\example.config.json

:: Create the release archive for Linux
cd ..
powershell Compress-Archive -Path "./LavaSharp-Linux-%COMMIT_HASH%" -DestinationPath "./LavaSharp-Linux-x86_64-%COMMIT_HASH%.zip"

:: Clean up Linux build
echo Cleaning up...

rmdir /s /q .\LavaSharp-Linux-%COMMIT_HASH%
rmdir /s /q .\LavaSharp-Windows-%COMMIT_HASH%
rmdir /s /q .\LavaSharp

echo The repository has been cloned and built twice (for Windows and Linux).
pause
