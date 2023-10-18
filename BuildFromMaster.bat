@echo off

set REPO_URL=https://github.com/FabiChan99/LavaSharp.git
set COMMIT_HASH=

git clone %REPO_URL%
cd LavaSharp

for /f "tokens=*" %%a in ('git rev-parse HEAD') do set COMMIT_HASH=%%a

dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=false

:: Step 4: Build for Linux 64-bit
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=false

:: Return to the original directory
cd ..

:: Step 5: Create the release directory
mkdir LavaSharp-%COMMIT_HASH%
cd LavaSharp-%COMMIT_HASH%

:: Step 6: Copy the Windows build
mkdir win-x64
cd win-x64
copy ..\LavaSharp\bin\Release

:: Step 7: Copy the Linux build
cd ..
mkdir linux-x64
cd linux-x64
copy ..\LavaSharp\bin\Release

:: Step 7.1 Copy the Example Config
cd ..
copy ..\LavaSharp\exampleconfig.json


:: Step 8: Create the release archive
cd ..
tar -czvf LavaSharp-%COMMIT_HASH%.tar.gz LavaSharp-%COMMIT_HASH%





echo The repository has been cloned and built twice (for Windows and Linux).
