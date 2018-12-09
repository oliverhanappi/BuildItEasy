using System;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class OrderLineBuilder : Builder<OrderLine, OrderLineBuilder>
    {
        private readonly Order _order;
        private readonly Property<Product> _product = RequiredProperty<Product>(new ProductBuilder());
        private readonly Property<int> _quantity = RequiredProperty<int>(1).Validate(v => v >= 1);

        public OrderLineBuilder(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));
        }

        public OrderLineBuilder WithProduct(Product product) => SetValue(_product, product);
        public OrderLineBuilder WithQuantity(int quantity) => SetValue(_quantity, quantity);
        
        public override OrderLine Build()
        {
            return _order.AddOrderLine(_product, _quantity);
        }
    }
}