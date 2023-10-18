@echo off
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

:: Create the release directory
mkdir LavaSharp-%COMMIT_HASH%
cd LavaSharp-%COMMIT_HASH%

:: Copy the Windows build
echo Copying Windows build...
mkdir win-x64
cd win-x64
copy ..\..\LavaSharp\bin\Release\net7.0\win-x64\publish\LavaSharp.exe

:: Copy the Linux build
cd ..
echo Copying Linux build...
mkdir linux-x64
cd linux-x64
copy ..\..\LavaSharp\bin\Release\net7.0\linux-x64\publish\LavaSharp

:: Copy the Example Config
cd ..
echo Copying Example Config...
copy ..\LavaSharp\exampleconfig.json

:: Create the release archive
cd ..
echo Creating release archive...
tar -czvf LavaSharp-%COMMIT_HASH%.tar.gz LavaSharp-%COMMIT_HASH%

:: Clean up
echo Cleaning up...
rmdir /s /q .\LavaSharp-%COMMIT_HASH%
rmdir /s /q .\LavaSharp

echo The repository has been cloned and built twice (for Windows and Linux).
pause
