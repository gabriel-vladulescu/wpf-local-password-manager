# PassVault Account Extractor

A standalone Python GUI tool for extracting and formatting account data from decrypted PassVault JSON files. This tool takes your decrypted PassVault data and creates a beautifully formatted text file with all your account information.

## Features

- 📄 **Extract Account Data** - Processes decrypted PassVault JSON files
- 🎨 **Beautiful Formatting** - Creates well-organized, readable text output
- 🔐 **Security Options** - Choose to include or hide passwords
- 📊 **Statistics** - Shows detailed account statistics
- 🗂️ **Group Filtering** - Extract accounts from specific groups only
- 📦 **Archive Support** - Option to include archived/trashed accounts
- 🖥️ **User-friendly GUI** - Simple tkinter-based interface

## Requirements

- Python 3.7 or higher
- tkinter (usually included with Python)
- No additional dependencies required

## Installation

1. **Navigate to the Extractor directory**:
   ```bash
   cd src/Tools/Extractor
   ```

2. **No additional installation needed** - uses only Python standard library

## Usage

### Starting the Tool

```bash
python extract_accounts.py
```

### Step-by-Step Process

1. **Launch the application**
   - Run the Python script to open the GUI

2. **Select decrypted JSON file**
   - Click "Browse..." to select your **decrypted** PassVault JSON file
   - The tool will analyze and show statistics about your data

3. **Configure export options**
   - **Include passwords**: Toggle password visibility in output
   - **Include archived accounts**: Include accounts marked as archived
   - **Include trashed accounts**: Include accounts marked as trashed
   - **Group filter**: Extract accounts from a specific group only

4. **Extract accounts**
   - Click "Extract Accounts" to generate the formatted text
   - Choose location and filename for the output file

## Output Format

The tool generates a beautifully formatted text file with:

```
================================================================================
PASSVAULT ACCOUNT EXPORT
================================================================================
Generated: 2024-01-15 14:30:22
Total Accounts: 25
================================================================================

📁 GROUP: Personal
--------------------------------------------------

🔐 1. My Email Account ⭐
   👤 Username: john.doe@email.com
   🔑 Password: SecurePass123!
   📧 Email: john.doe@email.com
   🌐 Website: https://email.provider.com
   📝 Notes: Main email account
           Recovery: +1-555-0123
   📅 Created: 2023-05-15 09:30:00
   📝 Modified: 2024-01-10 16:45:22

🔐 2. Banking Login
   👤 Username: john_doe_customer
   🔑 Password: ********** (hidden)
   🌐 Website: https://mybank.com
   📅 Created: 2023-08-22 11:15:30
```

## Account Fields Extracted

The tool extracts these fields from each account:

- **name** - Account display name
- **username** - Login username
- **password** - Account password (optional in output)
- **email** - Associated email address
- **website** - Account website/URL
- **notes** - Additional notes (with line break support)
- **group** - Group/category the account belongs to
- **is_favorite** - Favorite status (⭐ indicator)
- **is_archived** - Archive status (📦 indicator)
- **is_trashed** - Trash status (🗑️ indicator)
- **created_date** - Account creation date
- **last_modified** - Last modification date

## Export Options

### Security Options
- **Include passwords**: Show actual passwords vs asterisks
- **Password masking**: When hidden, shows `**********` pattern

### Filtering Options
- **Group filter**: Export only accounts from specific groups
- **Archive inclusion**: Include/exclude archived accounts
- **Trash inclusion**: Include/exclude trashed accounts

### Output Features
- **Emoji indicators**: Visual icons for different data types
- **Status badges**: ⭐ Favorite, 📦 Archived, 🗑️ Trashed
- **Grouping**: Accounts organized by their groups
- **Statistics**: Header with account counts and totals
- **Timestamps**: Export date and account modification dates

## File Format Support

The tool works with decrypted PassVault JSON files containing:

```json
{
  "Groups": [
    {
      "Name": "Personal",
      "Accounts": [
        {
          "name": "My Account",
          "username": "user123",
          "password": "secret123",
          "email": "user@example.com",
          "website": "https://example.com",
          "notes": "Account notes here"
        }
      ]
    }
  ]
}
```

## Statistics Display

The tool shows comprehensive statistics:

- **Total Accounts**: Count of all accounts
- **Active Accounts**: Non-archived, non-trashed accounts
- **Favorite Accounts**: Accounts marked as favorites
- **Archived/Trashed**: Counts by status
- **Data Completeness**: Accounts with passwords, emails, websites, notes

## Use Cases

### Personal Backup
- Create readable backups of your password data
- Generate emergency access documents (without passwords)
- Create printed copies for secure storage

### Data Migration
- Export data for import into other password managers
- Create CSV-compatible format for spreadsheet import
- Generate reports for security audits

### Team Sharing
- Share selected account groups with team members
- Create sanitized versions without sensitive passwords
- Generate documentation for shared accounts

## Security Notes

⚠️ **Important Security Considerations**:

- This tool processes **decrypted** data only
- Output files contain **plain text** account information
- When passwords are included, they are stored in **plain text**
- Always secure the output files appropriately
- Consider using password hiding option for sensitive exports
- Delete output files securely when no longer needed

## Troubleshooting

### Common Issues

**"No accounts found" error**:
- Ensure you're using a **decrypted** PassVault JSON file
- Check that the JSON structure contains "Groups" with "Accounts"
- Verify the file was properly decrypted using the Decryptor tool

**"Invalid JSON file" error**:
- Make sure the file is valid JSON format
- Check for file corruption during decryption
- Try re-decrypting the original encrypted file

**Missing account data**:
- Some fields may be empty/null in the original data
- The tool handles missing fields gracefully
- Check the original PassVault data for completeness

### Getting Help

If you encounter issues:
1. Verify your JSON file is properly decrypted
2. Check the file contains the expected PassVault structure
3. Test with a known-good decrypted file
4. Ensure Python 3.7+ is installed

## Technical Implementation

The tool uses:

- **Field extraction**: Case-insensitive field detection
- **Data validation**: Handles missing/null values gracefully
- **Unicode support**: Full UTF-8 encoding for international characters
- **Date formatting**: Automatic date/time formatting
- **Group organization**: Automatic sorting by group and name
- **Memory efficient**: Processes large account databases efficiently

## License

This tool is part of the PassVault project and follows the same license terms.