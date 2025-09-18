@echo off
REM PassVault Account Extractor Launcher
REM This batch file helps launch the Python account extractor on Windows

echo PassVault Account Extractor
echo ===========================

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.7+ from https://python.org
    pause
    exit /b 1
)

REM Check if we're in the right directory
if not exist "extract_accounts.py" (
    echo ERROR: extract_accounts.py not found
    echo Please run this script from the Extractor directory
    pause
    exit /b 1
)

REM Install/upgrade required dependencies automatically
echo Installing/updating required dependencies...
pip install ttkbootstrap>=1.10.0 --upgrade --quiet
if errorlevel 1 (
    echo ERROR: Failed to install dependencies
    echo Please check your internet connection and Python installation
    pause
    exit /b 1
)

echo.
echo Starting PassVault Account Extractor...
echo.
echo This tool extracts and formats account data from decrypted PassVault JSON files.
echo Make sure you have already decrypted your PassVault file using the Decryptor tool.
echo.

python extract_accounts.py

pause