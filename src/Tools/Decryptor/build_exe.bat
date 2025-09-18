@echo off
REM PassVault Decryptor EXE Builder
REM This script builds a standalone EXE file for the Decryptor tool

echo PassVault Decryptor EXE Builder
echo =================================

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.7+ from https://python.org
    pause
    exit /b 1
)

REM Install PyInstaller if not available
echo Installing/updating PyInstaller...
pip install pyinstaller --upgrade --quiet
if errorlevel 1 (
    echo ERROR: Failed to install PyInstaller
    echo Please check your internet connection and Python installation
    pause
    exit /b 1
)

REM Create .build directory if it doesn't exist
if not exist ".build" mkdir .build

echo.
echo Building PassVault Decryptor...
echo ===============================

REM Build Decryptor EXE
pyinstaller --onefile --windowed --name "PassVault-Decryptor" --icon icon.ico --distpath ".build" decrypt_tool.py
if errorlevel 1 (
    echo ERROR: Failed to build Decryptor EXE
    pause
    exit /b 1
)

REM Clean up build files
rmdir /s /q "build" 2>nul
del "*.spec" 2>nul

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
if exist ".build\PassVault-Decryptor.exe" (
    echo ✓ PassVault-Decryptor.exe created in .build folder
) else (
    echo ERROR: PassVault-Decryptor.exe not found
)
echo.
echo The standalone EXE file is ready for distribution.
echo It doesn't require Python installation to run.
echo.

pause