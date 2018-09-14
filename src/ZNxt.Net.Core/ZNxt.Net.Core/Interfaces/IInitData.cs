using System;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IInitData
    {
        DateTime InitDateTime { get; }
        string TransactionId { get; }
    }
}