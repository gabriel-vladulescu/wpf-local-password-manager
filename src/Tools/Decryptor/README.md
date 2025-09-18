# PassVault Decryption Tool

A standalone Python GUI tool for decrypting files encrypted with PassVault's encryption structure. This tool allows you to access your encrypted PassVault data from outside the main application.

## Features

- 🔓 **Decrypt PassVault files** - Works with the same encryption algorithm and salt as the main application
- 🖥️ **User-friendly GUI** - Simple tkinter-based interface
- 📁 **File browser** - Easy file selection with drag-and-drop support
- 🔒 **Secure passphrase input** - Hidden password entry
- 💾 **Export decrypted data** - Save decrypted JSON files
- ⚡ **Fast decryption** - Uses the same AES-256-GCM encryption as PassVault

## Requirements

- Python 3.7 or higher
- tkinter (usually included with Python)
- cryptography library

## Installation

1. **Clone or download** the PassVault project
2. **Navigate to the Tools directory**:
   ```bash
   cd src/Tools
   ```
3. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

## Usage

### Starting the Tool

```bash
python decrypt_tool.py
```

### Step-by-Step Process

1. **Launch the application**
   - Run the Python script to open the GUI

2. **Select encrypted file**
   - Click "Browse..." to select your encrypted PassVault file
   - The tool will automatically detect if the file is encrypted

3. **Enter passphrase**
   - Click "Decrypt File"
   - Enter the same passphrase you use in PassVault
   - The tool will decrypt and display the content

4. **Save decrypted file** (optional)
   - Click "Save Decrypted File" to export the plain JSON
   - Choose location and filename for the decrypted data

## Encryption Details

This tool replicates PassVault's encryption implementation:

- **Algorithm**: AES-256-GCM
- **Key Derivation**: PBKDF2-SHA256 with 100,000 iterations
- **Salt**: Uses the same constant salt as PassVault (`PassVault2024_SecureSalt_v1.0.0`)
- **Structure**: Compatible with PassVault's `EncryptedContainer` format

## Security Notes

⚠️ **Important Security Considerations**:

- This tool uses the same security parameters as PassVault
- Your passphrase is never stored or logged
- Decrypted content is only held in memory during the session
- The tool works offline and doesn't transmit any data
- Always verify file integrity before and after decryption

## File Format Support

The tool supports PassVault encrypted files with these structures:

**Standard format (C# application)**:
```json
{
  "Data": "base64-encoded-encrypted-content",
  "Version": "1.0"
}
```

**Alternative format (lowercase fields)**:
```json
{
  "data": "base64-encoded-encrypted-content",
  "version": "1.0"
}
```

The tool automatically detects both field name variations and validates the base64 encrypted content.

## Troubleshooting

### Common Issues

**"Decryption failed" error**:
- Verify you're using the correct passphrase
- Ensure the file was encrypted with PassVault
- Check that the file isn't corrupted

**"Not a valid JSON file" error**:
- Make sure you selected an encrypted PassVault file
- The file should have `.json` extension
- File should contain `Data` and `Version` fields

**Import errors**:
- Install cryptography: `pip install cryptography`
- Ensure Python 3.7+ is installed
- On some systems: `pip3 install cryptography`

### Getting Help

If you encounter issues:
1. Check that you have the latest version of the tool
2. Verify your Python and dependency versions
3. Test with a known-good encrypted file
4. Check the PassVault application logs for encryption details

## Technical Implementation

The tool mirrors PassVault's C# encryption implementation:

- **Salt**: Constant salt from `AppConfig.Encryption.Salt`
- **Key derivation**: PBKDF2-SHA256, 100,000 iterations, 32-byte key
- **Encryption**: AES-256-GCM with 12-byte nonce and 16-byte tag
- **Structure**: `nonce + ciphertext + tag` encoded as Base64

## License

This tool is part of the PassVault project and follows the same license terms.