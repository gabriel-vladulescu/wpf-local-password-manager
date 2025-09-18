@echo off
REM PassVault Decryption Tool Launcher
REM This batch file helps launch the Python decryption tool on Windows

echo PassVault Decryption Tool
echo ========================

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.7+ from https://python.org
    pause
    exit /b 1
)

REM Check if we're in the right directory
if not exist "decrypt_tool.py" (
    echo ERROR: decrypt_tool.py not found
    echo Please run this script from the Tools directory
    pause
    exit /b 1
)

REM Install/upgrade required dependencies automatically
echo Installing/updating required dependencies...
pip install cryptography>=41.0.0 ttkbootstrap>=1.10.0 --upgrade --quiet
if errorlevel 1 (
    echo ERROR: Failed to install dependencies
    echo Please check your internet connection and Python installation
    pause
    exit /b 1
)

echo Starting PassVault Decryption Tool...
python decrypt_tool.py

pause