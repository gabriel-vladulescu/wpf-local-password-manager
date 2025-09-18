#!/bin/bash
# PassVault Account Extractor Launcher
# This script helps launch the Python account extractor on Unix-like systems

echo "PassVault Account Extractor"
echo "==========================="

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

# Check if we're in the right directory
if [ ! -f "extract_accounts.py" ]; then
    echo "ERROR: extract_accounts.py not found"
    echo "Please run this script from the Extractor directory"
    exit 1
fi

# Install/upgrade required dependencies automatically
echo "Installing/updating required dependencies..."
$PYTHON_CMD -m pip install "ttkbootstrap>=1.10.0" --upgrade --quiet
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install dependencies"
    echo "Please check your internet connection and Python installation"
    exit 1
fi

echo ""
echo "Starting PassVault Account Extractor..."
echo ""
echo "This tool extracts and formats account data from decrypted PassVault JSON files."
echo "Make sure you have already decrypted your PassVault file using the Decryptor tool."
echo ""

$PYTHON_CMD extract_accounts.py