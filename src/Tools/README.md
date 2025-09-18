# PassVault Tools

A collection of standalone tools for working with PassVault data outside the main application.

## 🔧 Available Tools

### 1. 🔓 **Decryptor** - Decrypt PassVault Files
**Location**: `Tools/Decryptor/`

Decrypt encrypted PassVault files using your passphrase. This tool allows you to access your encrypted data from outside the main application.

**Features**:
- Decrypt encrypted PassVault files
- GUI interface with file browser
- Compatible with PassVault encryption format
- Export decrypted JSON files

**Usage**:
```bash
cd Tools/Decryptor
python decrypt_tool.py
# Or use the launchers:
# Windows: run_decrypt_tool.bat
# Unix/Linux/Mac: ./run_decrypt_tool.sh
```

### 2. 📄 **Extractor** - Format Account Data
**Location**: `Tools/Extractor/`

Extract and format account data from decrypted PassVault JSON files into beautifully formatted text files.

**Features**:
- Extract all account information
- Beautiful text formatting with emojis
- Security options (hide/show passwords)
- Group filtering and statistics
- Archive/trash inclusion options

**Usage**:
```bash
cd Tools/Extractor
python extract_accounts.py
# Or use the launchers:
# Windows: run_extractor.bat
# Unix/Linux/Mac: ./run_extractor.sh
```

## 🔄 Typical Workflow

Here's how to use these tools together:

### Step 1: Decrypt Your Data
1. Use the **Decryptor** tool to decrypt your encrypted PassVault file
2. Enter your passphrase when prompted
3. Save the decrypted JSON file

### Step 2: Extract Account Information
1. Use the **Extractor** tool with the decrypted JSON file
2. Configure export options (passwords, groups, etc.)
3. Generate a formatted text file with your account data

## 📋 Requirements

Both tools require:
- **Python 3.7+**
- **tkinter** (usually included with Python)
- **Decryptor only**: `cryptography` library (`pip install cryptography`)

## 🛠️ Installation

1. **Clone or download** the PassVault project
2. **Navigate to the desired tool directory**:
   ```bash
   cd src/Tools/Decryptor    # For decryption
   cd src/Tools/Extractor    # For extraction
   ```
3. **Install dependencies** (Decryptor only):
   ```bash
   pip install -r requirements.txt
   ```

## 🔒 Security Notes

⚠️ **Important Security Considerations**:

### Decryptor Tool
- Uses the same encryption parameters as PassVault
- Your passphrase is never stored or logged
- Decrypted content is only held in memory during the session
- Works offline and doesn't transmit any data

### Extractor Tool
- Processes **decrypted** data only
- Output files contain **plain text** information
- When passwords are included, they are stored in **plain text**
- Always secure output files appropriately
- Consider using password hiding option for sensitive exports

### General Security
- Always verify file integrity before and after processing
- Delete temporary files securely when no longer needed
- Keep decrypted files in secure locations
- Use password hiding options when sharing extracted data

## 📝 Output Examples

### Decryptor Output
```json
{
  "Groups": [
    {
      "Name": "Personal",
      "Accounts": [
        {
          "name": "My Email",
          "username": "user@example.com",
          "password": "secretpass123",
          "email": "user@example.com",
          "website": "https://email.com"
        }
      ]
    }
  ],
  "Version": "2.2.0"
}
```

### Extractor Output
```
================================================================================
PASSVAULT ACCOUNT EXPORT
================================================================================
Generated: 2024-01-15 14:30:22
Total Accounts: 5
================================================================================

📁 GROUP: Personal
--------------------------------------------------

🔐 1. My Email ⭐
   👤 Username: user@example.com
   🔑 Password: secretpass123
   📧 Email: user@example.com
   🌐 Website: https://email.com
   📅 Created: 2023-05-15 09:30:00
```

## 🐛 Troubleshooting

### Common Issues

**Python not found**:
- Install Python 3.7+ from [python.org](https://python.org)
- Ensure Python is added to your system PATH

**Tool files not found**:
- Make sure you're running scripts from the correct directory
- Check that all tool files were properly downloaded/extracted

**Decryption fails**:
- Verify you're using the correct passphrase
- Ensure the file was encrypted with PassVault
- Check that the file isn't corrupted

**No accounts found in extraction**:
- Make sure you're using a **decrypted** JSON file
- Verify the JSON structure contains "Groups" with "Accounts"
- Try decrypting the file again if needed

### Getting Help

If you encounter issues:
1. Check the README in each tool's directory for specific instructions
2. Verify you have the correct Python version and dependencies
3. Test with known-good files
4. Check the PassVault application logs for any relevant information

## 🚀 Advanced Usage

### Batch Processing
You can modify the tools for batch processing multiple files:

```bash
# Example: Decrypt multiple files
for file in *.json; do
    python decrypt_tool.py "$file"
done
```

### Integration with Other Tools
The tools can be integrated into workflows:
- Use in backup scripts
- Integrate with password managers
- Include in data migration processes

## 📄 License

These tools are part of the PassVault project and follow the same license terms.