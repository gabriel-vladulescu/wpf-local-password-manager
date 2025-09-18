#!/usr/bin/env python3
"""
PassVault Account Extractor

A GUI tool for extracting and formatting account data from decrypted PassVault JSON files.
This tool extracts account information and formats it into a readable text file.

Requirements:
- Python 3.7+
- tkinter (usually included with Python)

Usage:
python extract_accounts.py
"""

import json
import os
import sys
from typing import List, Dict, Any, Optional
from pathlib import Path
from datetime import datetime

try:
    import ttkbootstrap as ttk
    from ttkbootstrap.constants import *
    from ttkbootstrap.dialogs import Messagebox
    from tkinter import filedialog
except ImportError:
    print("Error: ttkbootstrap library is required. Install it with: pip install ttkbootstrap")
    sys.exit(1)


class AccountExtractor:
    """PassVault account data extraction and formatting"""

    @classmethod
    def extract_accounts_from_json(cls, data: Dict[Any, Any]) -> List[Dict[str, Any]]:
        """
        Extract all accounts from PassVault JSON data

        Args:
            data: Decrypted PassVault JSON data

        Returns:
            List of account dictionaries with standardized fields
        """
        accounts = []

        if not isinstance(data, dict):
            return accounts

        # Extract from Groups structure
        groups = data.get('Groups', data.get('groups', []))
        if not isinstance(groups, list):
            return accounts

        for group in groups:
            if not isinstance(group, dict):
                continue

            group_name = group.get('Name', group.get('name', 'Unknown Group'))
            group_accounts = group.get('Accounts', group.get('accounts', []))

            if not isinstance(group_accounts, list):
                continue

            for account in group_accounts:
                if not isinstance(account, dict):
                    continue

                # Extract account fields with case-insensitive handling
                extracted_account = {
                    'group': group_name,
                    'name': cls._get_field(account, ['Name', 'name']),
                    'username': cls._get_field(account, ['Username', 'username']),
                    'password': cls._get_field(account, ['Password', 'password']),
                    'email': cls._get_field(account, ['Email', 'email']),
                    'website': cls._get_field(account, ['Website', 'website']),
                    'notes': cls._get_field(account, ['Notes', 'notes']),
                    'is_favorite': cls._get_field(account, ['IsFavorite', 'isFavorite', 'is_favorite'], default=False),
                    'is_archived': cls._get_field(account, ['IsArchived', 'isArchived', 'is_archived'], default=False),
                    'is_trashed': cls._get_field(account, ['IsTrashed', 'isTrashed', 'is_trashed'], default=False),
                    'created_date': cls._get_field(account, ['CreatedDate', 'createdDate', 'created_date']),
                    'last_modified': cls._get_field(account, ['LastModified', 'lastModified', 'last_modified'])
                }

                accounts.append(extracted_account)

        return accounts

    @classmethod
    def _get_field(cls, data: Dict[str, Any], field_names: List[str], default: Any = None) -> Any:
        """Get field value with case-insensitive fallback"""
        for field in field_names:
            if field in data:
                value = data[field]
                # Return None for empty strings, otherwise return the value
                return value if value not in [None, "", "null"] else default
        return default

    @classmethod
    def format_accounts_as_text(cls, accounts: List[Dict[str, Any]], include_passwords: bool = True,
                              include_archived: bool = False, include_trashed: bool = False,
                              group_filter: Optional[str] = None) -> str:
        """
        Format accounts as readable text

        Args:
            accounts: List of account dictionaries
            include_passwords: Whether to include passwords in output
            include_archived: Whether to include archived accounts
            include_trashed: Whether to include trashed accounts
            group_filter: Only include accounts from this group (None for all)

        Returns:
            Formatted text string
        """
        if not accounts:
            return "No accounts found in the data.\n"

        # Filter accounts based on criteria
        filtered_accounts = []
        for account in accounts:
            # Skip archived/trashed if not requested
            if not include_archived and account.get('is_archived', False):
                continue
            if not include_trashed and account.get('is_trashed', False):
                continue

            # Filter by group if specified
            if group_filter and account.get('group', '').lower() != group_filter.lower():
                continue

            filtered_accounts.append(account)

        if not filtered_accounts:
            return "No accounts match the specified criteria.\n"

        # Sort accounts by group, then by name
        filtered_accounts.sort(key=lambda x: (x.get('group', ''), x.get('name', '')))

        # Generate formatted output
        output = []
        output.append("=" * 80)
        output.append("PASSVAULT ACCOUNT EXPORT")
        output.append("=" * 80)
        output.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        output.append(f"Total Accounts: {len(filtered_accounts)}")

        if group_filter:
            output.append(f"Group Filter: {group_filter}")
        if not include_passwords:
            output.append("NOTE: Passwords are hidden for security")
        if include_archived:
            output.append("NOTE: Including archived accounts")
        if include_trashed:
            output.append("NOTE: Including trashed accounts")

        output.append("=" * 80)
        output.append("")

        # Group accounts by group name
        current_group = None
        account_count = 0

        for account in filtered_accounts:
            group_name = account.get('group', 'Unknown Group')

            # Add group header if changed
            if current_group != group_name:
                if current_group is not None:
                    output.append("")
                output.append(f"📁 GROUP: {group_name}")
                output.append("-" * 50)
                current_group = group_name

            account_count += 1

            # Account header
            name = account.get('name', 'Unnamed Account')
            status_indicators = []

            if account.get('is_favorite', False):
                status_indicators.append("⭐")
            if account.get('is_archived', False):
                status_indicators.append("📦 ARCHIVED")
            if account.get('is_trashed', False):
                status_indicators.append("🗑️ TRASHED")

            status_text = " " + " ".join(status_indicators) if status_indicators else ""
            output.append(f"\n🔐 {account_count}. {name}{status_text}")

            # Account details
            details = []

            username = account.get('username')
            if username:
                details.append(f"   👤 Username: {username}")

            if include_passwords:
                password = account.get('password')
                if password:
                    details.append(f"   🔑 Password: {password}")
            else:
                password = account.get('password')
                if password:
                    details.append(f"   🔑 Password: {'*' * len(password)} (hidden)")

            email = account.get('email')
            if email:
                details.append(f"   📧 Email: {email}")

            website = account.get('website')
            if website:
                details.append(f"   🌐 Website: {website}")

            notes = account.get('notes')
            if notes:
                # Format notes with proper line breaks
                notes_lines = notes.replace('\r\n', '\n').replace('\r', '\n').split('\n')
                details.append(f"   📝 Notes:")
                for line in notes_lines:
                    details.append(f"      {line}")

            # Dates
            created = account.get('created_date')
            if created:
                details.append(f"   📅 Created: {cls._format_date(created)}")

            modified = account.get('last_modified')
            if modified:
                details.append(f"   📝 Modified: {cls._format_date(modified)}")

            if not details:
                details.append("   (No additional details)")

            output.extend(details)

        output.append("")
        output.append("=" * 80)
        output.append(f"Export completed. Total accounts exported: {len(filtered_accounts)}")
        output.append("=" * 80)

        return "\n".join(output)

    @classmethod
    def _format_date(cls, date_str: str) -> str:
        """Format date string for display"""
        if not date_str:
            return "Unknown"

        try:
            # Try parsing ISO format
            if 'T' in date_str:
                dt = datetime.fromisoformat(date_str.replace('Z', '+00:00'))
                return dt.strftime('%Y-%m-%d %H:%M:%S')
            else:
                return date_str
        except Exception:
            return date_str

    @classmethod
    def get_groups_from_accounts(cls, accounts: List[Dict[str, Any]]) -> List[str]:
        """Get unique group names from accounts"""
        groups = set()
        for account in accounts:
            group = account.get('group')
            if group:
                groups.add(group)
        return sorted(list(groups))

    @classmethod
    def get_account_statistics(cls, accounts: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Get statistics about the accounts"""
        if not accounts:
            return {}

        stats = {
            'total_accounts': len(accounts),
            'active_accounts': len([a for a in accounts if not a.get('is_archived', False) and not a.get('is_trashed', False)]),
            'favorite_accounts': len([a for a in accounts if a.get('is_favorite', False)]),
            'archived_accounts': len([a for a in accounts if a.get('is_archived', False)]),
            'trashed_accounts': len([a for a in accounts if a.get('is_trashed', False)]),
            'accounts_with_passwords': len([a for a in accounts if a.get('password')]),
            'accounts_with_emails': len([a for a in accounts if a.get('email')]),
            'accounts_with_websites': len([a for a in accounts if a.get('website')]),
            'accounts_with_notes': len([a for a in accounts if a.get('notes')]),
            'total_groups': len(cls.get_groups_from_accounts(accounts))
        }

        return stats


class AccountExtractorGUI:
    """GUI application for the PassVault account extractor"""

    def __init__(self):
        self.root = ttk.Window(
            title="PassVault Account Extractor",
            themename="darkly",  # Modern dark theme
            size=(900, 730),
            resizable=(True, True)
        )
        self.root.minsize(900, 730)

        # Set custom icon from local folder
        try:
            icon_path = Path(__file__).parent / "icon.ico"
            if icon_path.exists():
                self.root.iconbitmap(str(icon_path))
            else:
                # Fallback: remove default Python icon
                self.root.iconbitmap()
                self.root.wm_iconbitmap(default="")
        except:
            # Fallback: remove default Python icon
            try:
                self.root.iconbitmap()
                self.root.wm_iconbitmap(default="")
            except:
                pass

        # Variables
        self.selected_file_path = ttk.StringVar()
        self.file_content = None
        self.extracted_accounts = []
        self.include_passwords = ttk.BooleanVar(value=True)
        self.include_archived = ttk.BooleanVar(value=False)
        self.include_trashed = ttk.BooleanVar(value=False)
        self.selected_group = ttk.StringVar(value="All Groups")

        self.setup_ui()

    def setup_ui(self):
        """Setup the user interface"""
        # Main container with minimal top padding
        main_container = ttk.Frame(self.root, padding=(30, 15, 30, 30))
        main_container.pack(fill=BOTH, expand=True)

        # Skip header section - remove title for cleaner interface

        # File selection section
        file_section = ttk.LabelFrame(
            main_container,
            text="📁 File Selection",
            padding=20,
            bootstyle="info"
        )
        file_section.pack(fill=X, pady=(0, 20))

        file_label = ttk.Label(
            file_section,
            text="Select your decrypted PassVault JSON file:",
            font=("Segoe UI", 11)
        )
        file_label.pack(anchor=W, pady=(0, 10))

        file_frame = ttk.Frame(file_section)
        file_frame.pack(fill=X)

        self.browse_btn = ttk.Button(
            file_frame,
            text="📂 Browse Files",
            command=self.browse_file,
            bootstyle="outline-primary",
            width=15
        )
        self.browse_btn.pack(side=LEFT, padx=(0, 15))

        self.file_entry = ttk.Entry(
            file_frame,
            textvariable=self.selected_file_path,
            state='readonly',
            font=("Segoe UI", 10)
        )
        self.file_entry.pack(side=LEFT, fill=X, expand=True)

        # File information section with modern styling
        info_section = ttk.LabelFrame(
            main_container,
            text="📊 File Statistics",
            padding=20,
            bootstyle="info"
        )
        info_section.pack(fill=BOTH, expand=True, pady=(0, 20))

        # Statistics display with scrollbar
        stats_frame = ttk.Frame(info_section)
        stats_frame.pack(fill=BOTH, expand=True)

        from tkinter import Text
        self.stats_text = Text(
            stats_frame,
            height=6,
            wrap='word',
            state=DISABLED,
            font=("Consolas", 10),
            bg="#1a1a1a",
            fg="#00ff88",
            insertbackground="#00ff88",
            selectbackground="#3498db",
            relief="flat",
            padx=15,
            pady=10
        )

        stats_scrollbar = ttk.Scrollbar(stats_frame, orient="vertical", command=self.stats_text.yview)
        self.stats_text.configure(yscrollcommand=stats_scrollbar.set)

        self.stats_text.pack(side=LEFT, fill=BOTH, expand=True)
        stats_scrollbar.pack(side=RIGHT, fill=Y)

        # Export options section
        options_section = ttk.LabelFrame(
            main_container,
            text="⚙️ Export Options",
            padding=20,
            bootstyle="warning"
        )
        options_section.pack(fill=X, pady=(0, 20))

        # Options grid
        options_grid = ttk.Frame(options_section)
        options_grid.pack(fill=X, pady=(0, 15))

        # Security options row
        security_frame = ttk.Frame(options_grid)
        security_frame.pack(fill=X, pady=(0, 10))

        ttk.Checkbutton(
            security_frame,
            text="🔑 Include passwords",
            variable=self.include_passwords,
            bootstyle="success-round-toggle"
        ).pack(side=LEFT, padx=(0, 20))

        ttk.Checkbutton(
            security_frame,
            text="📦 Include archived accounts",
            variable=self.include_archived,
            bootstyle="info-round-toggle"
        ).pack(side=LEFT, padx=(0, 20))

        ttk.Checkbutton(
            security_frame,
            text="🗑️ Include trashed accounts",
            variable=self.include_trashed,
            bootstyle="danger-round-toggle"
        ).pack(side=LEFT)

        # Group filter section
        filter_frame = ttk.Frame(options_grid)
        filter_frame.pack(fill=X)

        ttk.Label(
            filter_frame,
            text="🗂️ Group filter:",
            font=("Segoe UI", 11, "bold")
        ).pack(side=LEFT, padx=(0, 15))

        self.group_combo = ttk.Combobox(
            filter_frame,
            textvariable=self.selected_group,
            state='readonly',
            font=("Segoe UI", 10),
            width=20
        )
        self.group_combo.pack(side=LEFT)

        # Action section
        action_section = ttk.Frame(main_container)
        action_section.pack(fill=X, pady=(0, 20))

        # Extract button with modern styling
        self.extract_btn = ttk.Button(
            action_section,
            text="📤 Extract Accounts",
            command=self.extract_accounts,
            state=DISABLED,
            bootstyle="success",
            width=25
        )
        self.extract_btn.pack(pady=15)

        # Progress bar with modern styling
        self.progress = ttk.Progressbar(
            action_section,
            mode='indeterminate',
            bootstyle="success-striped"
        )
        self.progress.pack(fill=X, pady=(10, 0))

        # Status bar with modern styling
        status_frame = ttk.Frame(main_container)
        status_frame.pack(fill=X, pady=(10, 0))

        self.status_var = ttk.StringVar()
        self.status_var.set("🟢 Ready - Select a decrypted PassVault JSON file to begin")

        self.status_bar = ttk.Label(
            status_frame,
            textvariable=self.status_var,
            font=("Segoe UI", 10),
            bootstyle="secondary",
            relief="flat",
            anchor=W
        )
        self.status_bar.pack(fill=X)

    def browse_file(self):
        """Open file dialog to select JSON file"""
        file_path = filedialog.askopenfilename(
            title="Select Decrypted PassVault JSON File",
            filetypes=[
                ("JSON files", "*.json"),
                ("All files", "*.*")
            ]
        )

        if file_path:
            self.selected_file_path.set(file_path)
            self.load_file_info(file_path)

    def load_file_info(self, file_path: str):
        """Load and analyze file content"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read().strip()

            # Parse JSON
            try:
                self.file_content = json.loads(content)
                self.extracted_accounts = AccountExtractor.extract_accounts_from_json(self.file_content)

                if not self.extracted_accounts:
                    self.status_var.set("❌ No accounts found in file")
                    self.extract_btn.config(state=DISABLED)
                    self.update_stats_display("No accounts found in the selected file.")
                    return

                # Update UI with file info
                self.update_stats_display()
                self.update_group_filter()
                self.extract_btn.config(state=NORMAL)
                self.status_var.set(f"📊 Loaded {len(self.extracted_accounts)} accounts from {Path(file_path).name}")

            except json.JSONDecodeError as e:
                Messagebox.show_error(f"Invalid JSON file: {str(e)}", "JSON Error")
                self.extract_btn.config(state=DISABLED)
                self.status_var.set("❌ Invalid JSON file")

        except Exception as e:
            Messagebox.show_error(f"Failed to load file: {str(e)}", "Load Error")
            self.extract_btn.config(state=DISABLED)
            self.status_var.set("❌ Error loading file")

    def update_stats_display(self, custom_message: str = None):
        """Update the statistics display"""
        self.stats_text.config(state=NORMAL)
        self.stats_text.delete(1.0, 'end')

        if custom_message:
            self.stats_text.insert(1.0, custom_message)
        elif self.extracted_accounts:
            stats = AccountExtractor.get_account_statistics(self.extracted_accounts)
            stats_text = f"""File Statistics:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Total Accounts: {stats['total_accounts']}
🟢 Active Accounts: {stats['active_accounts']}
⭐ Favorite Accounts: {stats['favorite_accounts']}
📦 Archived Accounts: {stats['archived_accounts']}
🗑️ Trashed Accounts: {stats['trashed_accounts']}
📁 Total Groups: {stats['total_groups']}

📋 Data Completeness:
   🔑 With Passwords: {stats['accounts_with_passwords']}
   📧 With Email: {stats['accounts_with_emails']}
   🌐 With Website: {stats['accounts_with_websites']}
   📝 With Notes: {stats['accounts_with_notes']}"""
            self.stats_text.insert(1.0, stats_text)

        self.stats_text.config(state=DISABLED)

    def update_group_filter(self):
        """Update the group filter combobox"""
        if not self.extracted_accounts:
            self.group_combo['values'] = ["All Groups"]
            return

        groups = AccountExtractor.get_groups_from_accounts(self.extracted_accounts)
        values = ["All Groups"] + groups
        self.group_combo['values'] = values
        self.selected_group.set("All Groups")

    def extract_accounts(self):
        """Extract and save accounts to text file"""
        if not self.extracted_accounts:
            Messagebox.show_error("No accounts to extract", "Extract Error")
            return

        # Get export options
        include_passwords = self.include_passwords.get()
        include_archived = self.include_archived.get()
        include_trashed = self.include_trashed.get()
        group_filter = self.selected_group.get() if self.selected_group.get() != "All Groups" else None

        # Start progress
        self.progress.start()
        self.status_var.set("🔄 Generating formatted text...")
        self.root.update()

        try:
            # Generate formatted text
            formatted_text = AccountExtractor.format_accounts_as_text(
                self.extracted_accounts,
                include_passwords=include_passwords,
                include_archived=include_archived,
                include_trashed=include_trashed,
                group_filter=group_filter
            )

            # Ask user where to save
            default_filename = f"passvault_accounts_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
            file_path = filedialog.asksaveasfilename(
                title="Save Extracted Accounts",
                defaultextension=".txt",
                initialfile=default_filename,
                filetypes=[
                    ("Text files", "*.txt"),
                    ("All files", "*.*")
                ]
            )

            if file_path:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(formatted_text)

                Messagebox.show_info(f"Accounts extracted successfully! 🎉\n\n📁 {file_path}", "Export Complete")
                self.status_var.set(f"✅ Exported to: {Path(file_path).name}")
            else:
                self.status_var.set("❌ Export cancelled")

        except Exception as e:
            Messagebox.show_error(f"Failed to extract accounts: {str(e)}", "Export Error")
            self.status_var.set("❌ Export failed")

        finally:
            self.progress.stop()

    def run(self):
        """Start the GUI application"""
        self.root.mainloop()


def main():
    """Main entry point"""
    try:
        app = AccountExtractorGUI()
        app.run()
    except KeyboardInterrupt:
        print("\nApplication terminated by user")
    except Exception as e:
        print(f"Error starting application: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()