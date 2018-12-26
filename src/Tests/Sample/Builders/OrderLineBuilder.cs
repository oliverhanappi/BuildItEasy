using System;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class OrderLineBuilder : Builder<OrderLine, OrderLineBuilder>
    {
        private readonly Order _order;
        private readonly Property<Product> _product;
        private readonly Property<int> _quantity;

        public OrderLineBuilder(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));

            _product = Property(l => l.Product, new ProductBuilder()).Required();
            _quantity = Property(l => l.Quantity, 1).Required().Validate(q => q >= 1);
        }

        public OrderLineBuilder WithProduct(Product product) => SetValue(_product, product);
        public OrderLineBuilder WithQuantity(int quantity) => SetValue(_quantity, quantity);

        protected override OrderLine BuildInternal()
        {
            return _order.AddOrderLine(_product, _quantity);
        }
    }
}
