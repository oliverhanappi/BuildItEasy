using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ProductBuilder : Builder<Product, ProductBuilder>
    {
        private static readonly Identity<string> ArticleNumberIdentity = new Identity<string>(i => $"{i:00000000}");
        private static readonly Identity<string> NameIdentity = new Identity<string>(i => $"Test Product {i}");

        private readonly Property<string> _articleNumber = RequiredProperty<string>(ArticleNumberIdentity);
        private readonly Property<string> _name = RequiredProperty<string>(NameIdentity);
        private readonly Property<decimal> _price = RequiredProperty<decimal>(41.99m).Validate(p => p > 0);
        private readonly Property<string> _description = Property<string>("This is an awesome product.");

        public ProductBuilder WithDescription(string description) => SetValue(_description, description);
        public ProductBuilder WithoutDescription() => NoValue(_description);
        
        public override Product Build()
        {
            return new Product
            {
                ArticleNumber = _articleNumber,
                Name = _name,
                Price = _price,
                Description = _description
            };
        }
    }
}
