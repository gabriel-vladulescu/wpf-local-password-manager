using System;

namespace AccountManager.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when data operations fail
    /// </summary>
    public class DataException : Exception
    {
        public DataException() : base() { }
        public DataException(string message) : base(message) { }
        public DataException(string message, Exception innerException) : base(message, innerException) { }
    }
}