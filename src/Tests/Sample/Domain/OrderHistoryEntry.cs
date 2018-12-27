using System;

namespace BuildItEasy.Tests.Sample.Domain
{
    public class OrderHistoryEntry
    {
        public Order Order { get; }
        public DateTimeOffset DateTime { get; }
        public string Message { get; }

        public OrderHistoryEntry(Order order, DateTimeOffset dateTime, string message)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            DateTime = dateTime;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}