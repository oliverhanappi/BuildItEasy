using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ProductBuilder : Builder<Product, ProductBuilder>
    {
        private static readonly Identity<string> ArticleNumberIdentity = new Identity<string>(i => $"{i:00000000}");
        private static readonly Identity<string> NameIdentity = new Identity<string>(i => $"Test Product {i}");

        private readonly Property<string> _articleNumber;
        private readonly Property<string> _name;
        private readonly Property<decimal> _price;
        private readonly Property<string> _description;

        public ProductBuilder()
        {
            _articleNumber = Property(p => p.ArticleNumber, ArticleNumberIdentity).Required();
            _name = Property(p => p.Name, NameIdentity).Required();
            _price = Property(p => p.Price, 41.99m).Required().Validate(p => p >= 0);
            _description = Property(p => p.Description, "This is an awesome product.");
        }

        public ProductBuilder WithArticleNumber(string articleNumber) => SetValue(_articleNumber, articleNumber);

        public ProductBuilder WithName(string name) => SetValue(_name, name);

        public ProductBuilder WithPrice(decimal price) => SetValue(_price, price);

        public ProductBuilder WithDescription(string description) => SetValue(_description, description);
        public ProductBuilder WithoutDescription() => NoValue(_description);

        protected override Product BuildInternal()
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