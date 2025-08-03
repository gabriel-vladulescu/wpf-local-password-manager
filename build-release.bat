@echo off
echo Building Account Manager...

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please download and install .NET 8 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo .NET SDK found, continuing build...
echo.

REM Clean up old build directories first
echo Cleaning up old build artifacts...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "publish" rmdir /s /q "publish"
if exist ".build" rmdir /s /q ".build"

REM Restore packages
echo Restoring packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

REM Build the application
echo Building application...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

REM Publish as single-file executable with all dependencies included
echo Publishing single-file executable with all dependencies...
dotnet publish -c Release --self-contained true -r win-x64 -o ./.build ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:PublishTrimmed=false ^
    /p:PublishReadyToRun=false ^
    /p:DebugType=None
if %errorlevel% neq 0 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)

REM Clean up build artifacts as requested
echo Cleaning up build artifacts...
if exist "bin" (
    echo Removing bin folder...
    rmdir /s /q "bin"
)
if exist "obj" (
    echo Removing obj folder...
    rmdir /s /q "obj"
)
if exist "publish" (
    echo Removing publish folder...
    rmdir /s /q "publish"
)

REM Clean up any remaining debug files
echo Cleaning up debug files...
if exist ".\.build\AccountManager.pdb" del ".\.build\AccountManager.pdb"
if exist ".\.build\*.pdb" del ".\.build\*.pdb"

REM Verify the executable was created
if not exist ".\.build\AccountManager.exe" (
    echo ERROR: AccountManager.exe was not created!
    pause
    exit /b 1
)

echo.
echo ✅ BUILD SUCCESSFUL!
echo ========================================
echo Executable location: .\.build\AccountManager.exe
echo.
echo File details:
for %%I in (".\.build\AccountManager.exe") do (
    echo Size: %%~zI bytes ^(~%%~zI bytes^)
    echo Modified: %%~tI
)
echo.
echo 📁 Build artifacts cleaned up:
echo   - bin folder removed
echo   - obj folder removed  
echo   - publish folder removed
echo.
echo ✨ Ready to use! 
echo Copy AccountManager.exe from .build folder to any location and run it.
echo The application is fully self-contained with all dependencies included.
echo The accounts.json file will be created automatically on first run.
echo.
pause