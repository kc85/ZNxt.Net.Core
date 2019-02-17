using System;

namespace ZNxt.Net.Core.Exceptions
{
    public class DBConnectionException : ExceptionBase
    {
        public DBConnectionException(int errorCode, string message, Exception innerException = null)
            : base(errorCode, message, innerException)
        {
        }
    }
}