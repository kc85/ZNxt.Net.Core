using Newtonsoft.Json.Linq;
using System;

namespace ZNxt.Net.Core.Interfaces
{
    public interface ILogger
    {
        void Debug(string message, JObject logData = null);

        void Error(string message, Exception ex);

        void Error(string message, Exception ex = null, JObject logData = null);

        void Info(string message, JObject logData = null);

        void Transaction(JObject transactionData, TransactionState state);

        string TransactionId { get; }

        double TransactionStartTime { get;  }
    }

    public enum TransactionState
    {
        Start,
        Finish
    }
}