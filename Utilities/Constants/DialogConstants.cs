namespace AccountManager.Utilities.Constants
{
    public static class DialogConstants
    {
        public static class Titles
        {
            public const string CreateGroup = "Create New Group";
            public const string EditGroup = "Edit Group";
            public const string DeleteGroup = "Delete Group";
            public const string CreateAccount = "Create Account";
            public const string EditAccount = "Edit Account";
            public const string DeleteAccount = "Delete Account";
            public const string Settings = "Settings";
            public const string Confirmation = "Confirm Action";
            public const string Error = "Error";
            public const string Warning = "Warning";
            public const string Information = "Information";
        }

        public static class Messages
        {
            public const string UnsavedChanges = "You have unsaved changes. Do you want to save them before closing?";
            public const string DeleteGroupConfirmation = "Are you sure you want to delete this group? This action cannot be undone.";
            public const string DeleteAccountConfirmation = "Are you sure you want to delete this account? This action cannot be undone.";
            public const string InvalidData = "The provided data is invalid. Please check your input and try again.";
            public const string SaveError = "An error occurred while saving. Please try again.";
            public const string LoadError = "An error occurred while loading data. Please check the file and try again.";
            public const string NetworkError = "A network error occurred. Please check your connection and try again.";
            public const string UnexpectedError = "An unexpected error occurred. Please try again or contact support.";
        }

        public static class ButtonTexts
        {
            public const string OK = "OK";
            public const string Cancel = "Cancel";
            public const string Save = "Save";
            public const string Delete = "Delete";
            public const string Create = "Create";
            public const string Edit = "Edit";
            public const string Close = "Close";
            public const string Yes = "Yes";
            public const string No = "No";
            public const string Retry = "Retry";
            public const string Continue = "Continue";
        }

        public static class Icons
        {
            public const string Success = "CheckCircle";
            public const string Error = "AlertCircle";
            public const string Warning = "Alert";
            public const string Information = "Information";
            public const string Question = "Help";
            public const string Delete = "Delete";
            public const string Edit = "Pencil";
            public const string Create = "Plus";
            public const string Save = "ContentSave";
            public const string Cancel = "Close";
        }
    }
}