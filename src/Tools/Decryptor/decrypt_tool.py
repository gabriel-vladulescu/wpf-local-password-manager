#!/usr/bin/env python3
"""
PassVault Decryption Tool

A GUI tool for decrypting files encrypted with PassVault's encryption structure.
This tool replicates the C# encryption logic using the same salt and algorithms.

Requirements:
- Python 3.7+
- tkinter (usually included with Python)
- cryptography library

Install dependencies:
pip install cryptography

Usage:
python decrypt_tool.py
"""

import json
import base64
import os
import sys
from typing import Optional, Dict, Any
from pathlib import Path

try:
    import ttkbootstrap as ttk
    from ttkbootstrap.constants import *
    from ttkbootstrap.dialogs import Messagebox
    from tkinter import filedialog, simpledialog
except ImportError:
    print("Error: ttkbootstrap library is required. Install it with: pip install ttkbootstrap")
    sys.exit(1)

try:
    from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
    from cryptography.hazmat.primitives import hashes
    from cryptography.hazmat.primitives.ciphers.aead import AESGCM
    from cryptography.hazmat.backends import default_backend
except ImportError:
    print("Error: cryptography library is required. Install it with: pip install cryptography")
    sys.exit(1)


class PassVaultDecryptor:
    """PassVault decryption logic matching the C# implementation"""

    # Constants matching C# EncryptionService
    KEY_SIZE = 32  # 256 bits
    NONCE_SIZE = 12  # 96 bits for GCM
    TAG_SIZE = 16  # 128 bits
    ITERATIONS = 100000  # PBKDF2 iterations
    SALT = "PassVault2024_SecureSalt_v1.0.0"  # Constant salt from AppConfig.Encryption.Salt

    @classmethod
    def derive_key(cls, passphrase: str, salt_bytes: bytes) -> bytes:
        """Derive encryption key using PBKDF2-SHA256"""
        kdf = PBKDF2HMAC(
            algorithm=hashes.SHA256(),
            length=cls.KEY_SIZE,
            salt=salt_bytes,
            iterations=cls.ITERATIONS,
            backend=default_backend()
        )
        return kdf.derive(passphrase.encode('utf-8'))

    @classmethod
    def decrypt_data(cls, encrypted_data: str, passphrase: str) -> str:
        """
        Decrypt data using AES-256-GCM with the same structure as C# implementation

        Args:
            encrypted_data: Base64 encoded encrypted data (nonce + ciphertext + tag)
            passphrase: User passphrase for decryption

        Returns:
            Decrypted plaintext string

        Raises:
            Exception: If decryption fails
        """
        try:
            # Decode base64 data
            encrypted_bytes = base64.b64decode(encrypted_data)
            salt_bytes = cls.SALT.encode('utf-8')

            if len(encrypted_bytes) < cls.NONCE_SIZE + cls.TAG_SIZE:
                raise ValueError("Invalid encrypted data format")

            # Extract components (matching C# Buffer.BlockCopy logic)
            nonce = encrypted_bytes[:cls.NONCE_SIZE]
            ciphertext_and_tag = encrypted_bytes[cls.NONCE_SIZE:]
            ciphertext = ciphertext_and_tag[:-cls.TAG_SIZE]
            tag = ciphertext_and_tag[-cls.TAG_SIZE:]

            # Combine ciphertext and tag for AESGCM
            ciphertext_with_tag = ciphertext + tag

            # Derive key from passphrase and salt
            key = cls.derive_key(passphrase, salt_bytes)

            # Decrypt using AES-GCM
            aesgcm = AESGCM(key)
            plaintext = aesgcm.decrypt(nonce, ciphertext_with_tag, None)

            return plaintext.decode('utf-8')

        except Exception as e:
            raise Exception(f"Decryption failed: {str(e)}")


class PasswordDialog:
    """Custom password dialog for ttkbootstrap"""

    def __init__(self, parent, title="Password Required", prompt="Enter password:"):
        self.result = None
        self.parent = parent

        # Create dialog window with adjusted size (50px more height)
        self.dialog = ttk.Toplevel(parent)
        self.dialog.title(title)
        self.dialog.geometry("500x270")
        self.dialog.resizable(True, True)
        self.dialog.minsize(350, 200)
        self.dialog.transient(parent)
        self.dialog.grab_set()

        # Set custom icon from local folder
        try:
            icon_path = Path(__file__).parent / "icon.ico"
            if icon_path.exists():
                self.dialog.iconbitmap(str(icon_path))
            else:
                # Fallback: remove default Python icon
                self.dialog.iconbitmap()
                self.dialog.wm_iconbitmap(default="")
        except:
            # Fallback: remove default Python icon
            try:
                self.dialog.iconbitmap()
                self.dialog.wm_iconbitmap(default="")
            except:
                pass

        # Center the dialog
        self.dialog.update_idletasks()
        x = (self.dialog.winfo_screenwidth() // 2) - (500 // 2)
        y = (self.dialog.winfo_screenheight() // 2) - (270 // 2)
        self.dialog.geometry(f"500x270+{x}+{y}")

        # Main frame with better padding
        main_frame = ttk.Frame(self.dialog, padding=40)
        main_frame.pack(fill=BOTH, expand=True)

        # Icon and prompt with better layout
        prompt_frame = ttk.Frame(main_frame)
        prompt_frame.pack(fill=X, pady=(0, 25))

        # Prompt text - centered and wrapped properly
        ttk.Label(
            prompt_frame,
            text=prompt,
            font=("Segoe UI", 11),
            wraplength=450,
            justify="center"
        ).pack(fill=X, expand=True)

        # Password entry with larger font
        self.password_var = ttk.StringVar()
        self.password_entry = ttk.Entry(
            main_frame,
            textvariable=self.password_var,
            show="*",
            font=("Segoe UI", 14),
            width=35
        )
        self.password_entry.pack(fill=X, pady=(0, 30))
        self.password_entry.focus()

        # Buttons with 50/50 width layout
        button_frame = ttk.Frame(main_frame)
        button_frame.pack(fill=X)

        # Configure grid columns for 50/50 split
        button_frame.grid_columnconfigure(0, weight=1)
        button_frame.grid_columnconfigure(1, weight=1)

        ttk.Button(
            button_frame,
            text="OK",
            command=self.ok,
            bootstyle="primary"
        ).grid(row=0, column=0, sticky="ew", padx=(0, 7))

        ttk.Button(
            button_frame,
            text="Cancel",
            command=self.cancel,
            bootstyle="outline-secondary"
        ).grid(row=0, column=1, sticky="ew", padx=(7, 0))

        # Bind Enter key
        self.dialog.bind('<Return>', lambda e: self.ok())
        self.dialog.bind('<Escape>', lambda e: self.cancel())

    def ok(self):
        self.result = self.password_var.get()
        self.dialog.destroy()

    def cancel(self):
        self.result = None
        self.dialog.destroy()

    def show(self):
        self.dialog.wait_window()
        return self.result


class DecryptionToolGUI:
    """GUI application for the PassVault decryption tool"""

    def __init__(self):
        self.root = ttk.Window(
            title="PassVault Decryption Tool",
            themename="darkly",  # Dark modern theme
            size=(800, 800),
            resizable=(True, True)
        )
        self.root.minsize(800, 800)

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
        self.decrypted_content = None

        self.setup_ui()

    def setup_ui(self):
        """Setup the user interface"""
        # Main container with padding
        main_container = ttk.Frame(self.root, padding=25)
        main_container.pack(fill=BOTH, expand=True)

        # Skip header section - remove title and subtitle for cleaner interface

        # File information section
        file_section = ttk.LabelFrame(
            main_container,
            text="📁 File Information",
            padding=15,
            bootstyle="info"
        )
        file_section.pack(fill=X, pady=(0, 15))

        file_label = ttk.Label(
            file_section,
            text="Select your encrypted PassVault file:",
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

        # File analysis section
        info_section = ttk.LabelFrame(
            main_container,
            text="📊 File Analysis",
            padding=15,
            bootstyle="info"
        )
        info_section.pack(fill=X, pady=(0, 15))

        # Info grid
        info_grid = ttk.Frame(info_section)
        info_grid.pack(fill=X)

        # File size
        ttk.Label(info_grid, text="📏 Size:", font=("Segoe UI", 10, "bold")).grid(row=0, column=0, sticky=W, padx=(0, 10))
        self.file_size_label = ttk.Label(info_grid, text="-", font=("Segoe UI", 10))
        self.file_size_label.grid(row=0, column=1, sticky=W, padx=(0, 0))

        # Encryption status
        ttk.Label(info_grid, text="🔐 Status:", font=("Segoe UI", 10, "bold")).grid(row=1, column=0, sticky=W, padx=(0, 10), pady=(5, 0))
        self.encryption_status_label = ttk.Label(info_grid, text="No file selected", font=("Segoe UI", 10))
        self.encryption_status_label.grid(row=1, column=1, sticky=W, padx=(0, 0), pady=(5, 0))

        # Version
        ttk.Label(info_grid, text="🔥 Version:", font=("Segoe UI", 10, "bold")).grid(row=2, column=0, sticky=W, padx=(0, 10), pady=(5, 0))
        self.version_label = ttk.Label(info_grid, text="-", font=("Segoe UI", 10))
        self.version_label.grid(row=2, column=1, sticky=W, padx=(0, 0), pady=(5, 0))

        # Action section
        action_section = ttk.Frame(main_container)
        action_section.pack(fill=X, pady=(0, 15))

        # Decrypt button
        self.decrypt_btn = ttk.Button(
            action_section,
            text="🔓 Decrypt File",
            command=self.decrypt_file,
            state=DISABLED,
            bootstyle="success",
            width=20
        )
        self.decrypt_btn.pack(pady=10)

        # Progress bar
        self.progress = ttk.Progressbar(
            action_section,
            mode='indeterminate',
            bootstyle="success-striped"
        )
        self.progress.pack(fill=X, pady=(10, 0))

        # Results section
        results_section = ttk.LabelFrame(
            main_container,
            text="📄 Decryption Results",
            padding=15,
            bootstyle="success"
        )
        results_section.pack(fill=BOTH, expand=True, pady=(0, 15))

        # Results text area with modern styling
        from tkinter import Text
        self.results_text = Text(
            results_section,
            height=10,
            wrap='word',
            state=DISABLED,
            font=("Consolas", 9),
            bg="#2b3e50",
            fg="#ecf0f1",
            insertbackground="#ecf0f1",
            selectbackground="#3498db",
            relief="flat",
            padx=10,
            pady=10
        )

        # Scrollbar for text area
        text_scrollbar = ttk.Scrollbar(results_section, orient="vertical", command=self.results_text.yview)
        self.results_text.configure(yscrollcommand=text_scrollbar.set)

        self.results_text.pack(side=LEFT, fill=BOTH, expand=True)
        text_scrollbar.pack(side=RIGHT, fill=Y)

        # Button frame for save functionality
        button_frame = ttk.Frame(main_container)
        button_frame.pack(fill=X, pady=(0, 15))

        # Save button
        self.download_btn = ttk.Button(
            button_frame,
            text="💾 Save Decrypted File",
            command=self.save_decrypted_file,
            state=DISABLED,
            bootstyle="outline-success",
            width=20
        )
        self.download_btn.pack(side=LEFT)

        # Status bar with modern styling
        status_frame = ttk.Frame(main_container)
        status_frame.pack(fill=X)

        self.status_var = ttk.StringVar()
        self.status_var.set("🟢 Ready - Select an encrypted PassVault file to begin")

        self.status_bar = ttk.Label(
            status_frame,
            textvariable=self.status_var,
            font=("Segoe UI", 9),
            bootstyle="secondary",
            relief="flat",
            anchor=W
        )
        self.status_bar.pack(fill=X, pady=(10, 0))

    def browse_file(self):
        """Open file dialog to select encrypted file"""
        file_path = filedialog.askopenfilename(
            title="Select Encrypted PassVault File",
            filetypes=[
                ("JSON files", "*.json"),
                ("All files", "*.*")
            ]
        )

        if file_path:
            self.selected_file_path.set(file_path)
            self.load_file_info(file_path)

    def load_file_info(self, file_path: str):
        """Load and display file information"""
        try:
            # Get file size
            file_size = os.path.getsize(file_path)
            size_text = f"Size: {file_size:,} bytes"
            self.file_size_label.config(text=size_text)

            # Try to load and analyze file content
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read().strip()

            # Try to parse as JSON
            try:
                data = json.loads(content)
                if self.is_encrypted_passvault_file(data):
                    # This looks like an encrypted PassVault file
                    self.encryption_status_label.config(text="🔒 Encrypted PassVault file detected")

                    # Show encryption container version
                    version = self.get_version_from_file(data)
                    self.version_label.config(text=f"{version}")

                    self.file_content = data
                    self.decrypt_btn.config(state=NORMAL)
                else:
                    # Check if it's unencrypted PassVault data
                    if isinstance(data, dict) and 'Groups' in data:
                        version = data.get('Version', 'Unknown')
                        self.encryption_status_label.config(text="📖 Unencrypted PassVault file")
                        self.version_label.config(text=f"{version}")
                    else:
                        self.encryption_status_label.config(text="📄 Regular JSON file")
                        self.version_label.config(text="-")

                    self.file_content = data
                    self.decrypt_btn.config(state=DISABLED)

            except json.JSONDecodeError:
                self.encryption_status_label.config(text="❌ Not a valid JSON file")
                self.file_content = None
                self.decrypt_btn.config(state=DISABLED)

            self.status_var.set(f"📁 File loaded: {Path(file_path).name}")

        except Exception as e:
            Messagebox.show_error(f"Failed to load file: {str(e)}", "Load Error")
            self.encryption_status_label.config(text="❌ Error loading file")
            self.decrypt_btn.config(state=DISABLED)

    def is_encrypted_passvault_file(self, data: Dict[Any, Any]) -> bool:
        """
        Check if the JSON data represents an encrypted PassVault file
        Supports both 'Data'/'data' and 'Version'/'version' field variations
        """
        if not isinstance(data, dict):
            return False

        # Check for data field (case-insensitive)
        data_field = None
        for key in ['Data', 'data']:
            if key in data:
                data_field = data[key]
                break

        # Check for version field (case-insensitive)
        version_field = None
        for key in ['Version', 'version']:
            if key in data:
                version_field = data[key]
                break

        if not data_field or not version_field:
            return False

        # Additional validation: check if data looks like base64 encrypted content
        if isinstance(data_field, str) and len(data_field) > 50:
            try:
                # Try to decode as base64 to validate format
                base64.b64decode(data_field, validate=True)
                return True
            except Exception:
                return False

        return False

    def get_encrypted_data_from_file(self, file_content: Dict[Any, Any]) -> Optional[str]:
        """
        Extract the encrypted data from file content, handling case variations
        """
        if not isinstance(file_content, dict):
            return None

        # Try different case variations
        for key in ['Data', 'data']:
            if key in file_content:
                return file_content[key]

        return None

    def get_version_from_file(self, file_content: Dict[Any, Any]) -> str:
        """
        Extract the version from file content, handling case variations
        """
        if not isinstance(file_content, dict):
            return "Unknown"

        # Try different case variations
        for key in ['Version', 'version']:
            if key in file_content:
                return str(file_content[key])

        return "Unknown"

    def decrypt_file(self):
        """Decrypt the selected file"""
        if not self.file_content:
            Messagebox.show_error("No valid encrypted file selected", "Selection Error")
            return

        # Check if file is encrypted
        if not self.is_encrypted_passvault_file(self.file_content):
            Messagebox.show_info("This file doesn't appear to be encrypted", "File Info")
            return

        # Get passphrase from user
        password_dialog = PasswordDialog(
            self.root,
            title="Passphrase Required",
            prompt="Enter the passphrase to decrypt this file:"
        )
        passphrase = password_dialog.show()

        if not passphrase:
            return

        # Start progress indication
        self.progress.start()
        self.status_var.set("🔄 Decrypting file...")
        self.root.update()

        try:
            # Decrypt the data
            encrypted_data = self.get_encrypted_data_from_file(self.file_content)
            if not encrypted_data:
                Messagebox.show_error("No encrypted data found in file", "Data Error")
                return

            decrypted_content = PassVaultDecryptor.decrypt_data(encrypted_data, passphrase)

            # Parse decrypted JSON
            self.decrypted_content = json.loads(decrypted_content)

            # Display results
            self.display_decrypted_content(self.decrypted_content)

            # Update version info with decrypted data version
            if isinstance(self.decrypted_content, dict) and 'Version' in self.decrypted_content:
                app_version = self.decrypted_content['Version']
                container_version = self.get_version_from_file(self.file_content)
                self.version_label.config(text=f"Container: {container_version} | App: {app_version}")

            self.download_btn.config(state=NORMAL)
            self.status_var.set("✅ Decryption successful")

        except Exception as e:
            Messagebox.show_error(f"Failed to decrypt file: {str(e)}", "Decryption Failed")
            self.status_var.set("❌ Decryption failed")

        finally:
            self.progress.stop()

    def display_decrypted_content(self, content: Dict[Any, Any]):
        """Display the decrypted content in the text area"""
        self.results_text.config(state=NORMAL)
        self.results_text.delete(1.0, 'end')

        # Format JSON with indentation
        formatted_content = json.dumps(content, indent=2, ensure_ascii=False)

        self.results_text.insert(1.0, formatted_content)
        self.results_text.config(state=DISABLED)

    def save_decrypted_file(self):
        """Save the decrypted content to a file"""
        if not self.decrypted_content:
            Messagebox.show_error("No decrypted content to save", "Save Error")
            return

        # Ask user where to save
        file_path = filedialog.asksaveasfilename(
            title="Save Decrypted File",
            defaultextension=".json",
            filetypes=[
                ("JSON files", "*.json"),
                ("All files", "*.*")
            ]
        )

        if file_path:
            try:
                with open(file_path, 'w', encoding='utf-8') as f:
                    json.dump(self.decrypted_content, f, indent=2, ensure_ascii=False)

                Messagebox.show_info(f"Decrypted file saved successfully!\n\n📁 {file_path}", "Save Complete")
                self.status_var.set(f"💾 Saved: {Path(file_path).name}")

            except Exception as e:
                Messagebox.show_error(f"Failed to save file: {str(e)}", "Save Error")

    def run(self):
        """Start the GUI application"""
        self.root.mainloop()


def main():
    """Main entry point"""
    try:
        app = DecryptionToolGUI()
        app.run()
    except KeyboardInterrupt:
        print("\nApplication terminated by user")
    except Exception as e:
        print(f"Error starting application: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()