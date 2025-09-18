#!/bin/bash
# PassVault Decryptor EXE Builder
# This script builds a standalone executable for the Decryptor tool

echo "PassVault Decryptor EXE Builder"
echo "================================="

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    if ! command -v python &> /dev/null; then
        echo "ERROR: Python is not installed"
        echo "Please install Python 3.7+ from your package manager or https://python.org"
        exit 1
    fi
    PYTHON_CMD="python"
else
    PYTHON_CMD="python3"
fi

# Check Python version
PYTHON_VERSION=$($PYTHON_CMD --version 2>&1 | awk '{print $2}')
echo "Using Python $PYTHON_VERSION"

# Install PyInstaller if not available
echo "Installing/updating PyInstaller..."
$PYTHON_CMD -m pip install pyinstaller --upgrade --quiet
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install PyInstaller"
    echo "Please check your internet connection and Python installation"
    exit 1
fi

# Create .build directory if it doesn't exist
mkdir -p .build

echo ""
echo "Building PassVault Decryptor..."
echo "==============================="

# Build Decryptor executable
$PYTHON_CMD -m PyInstaller --onefile --windowed --name "PassVault-Decryptor" --icon icon.ico --distpath ".build" decrypt_tool.py
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build Decryptor executable"
    exit 1
fi

# Clean up build files
rm -rf "build" 2>/dev/null
rm -f *.spec 2>/dev/null

echo ""
echo "========================================"
echo "Build completed successfully!"
echo "========================================"
echo ""
if [ -f ".build/PassVault-Decryptor" ]; then
    echo "✓ PassVault-Decryptor created in .build folder"
else
    echo "ERROR: PassVault-Decryptor not found"
fi
echo ""
echo "The standalone executable is ready for distribution."
echo "It doesn't require Python installation to run."
echo ""