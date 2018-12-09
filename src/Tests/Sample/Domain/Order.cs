using System;
using System.Collections.Generic;

namespace BuildItEasy.Tests.Sample.Domain
{
    public class Order
    {
        public string OrderNumber { get; }

        public OrderState State { get; private set; }

        public Contact Contact { get; private set; }

        private readonly List<OrderLine> _orderLines = new List<OrderLine>();
        public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
        
        public string PaymentTransactionId { get; private set; }

        public Order(string orderNumber)
        {
            OrderNumber = orderNumber;
            State = OrderState.Open;
        }

        public void SetContact(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));
            
            if (Contact != null)
                throw new InvalidOperationException($"Another contact has already been set for {this}");
            
            if (State != OrderState.Open)
                throw new InvalidOperationException($"The order is not open anymore.");

            Contact = contact;
        }

        public OrderLine AddOrderLine(Product product, int quantity)
        {
            if (State != OrderState.Open)
                throw new InvalidOperationException($"Order lines cannot be added to {this} anymore.");

            var orderLine = new OrderLine(this, product, quantity);
            _orderLines.Add(orderLine);

            return orderLine;
        }

        public void CheckOut()
        {
            if (State != OrderState.Open)
                throw new InvalidOperationException($"{this} has already been checked out.");

            if (OrderLines.Count == 0)
                throw new InvalidOperationException($"{this} has no order lines.");

            if (Contact == null)
                throw new InvalidOperationException($"{this} has no contact.");

            State = OrderState.PaymentPending;
        }

        public void ConfirmPayment(string paymentTransactionId)
        {
            if (State != OrderState.PaymentPending)
                throw new InvalidOperationException($"Payment for {this} cannot be confirmed.");

            PaymentTransactionId = paymentTransactionId;
            State = OrderState.ShipmentPending;
        }

        public void Ship()
        {
            if (State != OrderState.ShipmentPending)
                throw new InvalidOperationException($"{this} has not been paid yet.");

            State = OrderState.Shipped;
        }

        public void Cancel()
        {
            if (State == OrderState.Canceled)
                throw new InvalidOperationException($"{this} has already been canceled.");
            
            State = OrderState.Canceled;
        }

        public override string ToString()
        {
            return $"Order #{OrderNumber}";
        }
    }
}
