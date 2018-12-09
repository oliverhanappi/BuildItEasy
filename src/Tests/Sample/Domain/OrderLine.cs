using System;

namespace BuildItEasy.Tests.Sample.Domain
{
    public class OrderLine
    {
        public Order Order { get; }
        public Product Product { get; }
        public int Quantity { get; }

        public OrderLine(Order order, Product product, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));
            
            Order = order ?? throw new ArgumentNullException(nameof(order));
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = quantity;
        }
    }
}
