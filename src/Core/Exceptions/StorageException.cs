using System;

namespace AccountManager.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when storage operations fail
    /// </summary>
    public class StorageException : Exception
    {
        public StorageException() : base() { }
        public StorageException(string message) : base(message) { }
        public StorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}