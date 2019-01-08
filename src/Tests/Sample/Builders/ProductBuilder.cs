using BuildItEasy.Identities;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ProductBuilder : Builder<Product, ProductBuilder>
    {
        private static readonly Identity<string> ArticleNumberIdentity = new Identity<string>(i => $"{i:00000000}");
        private static readonly Identity<string> NameIdentity = new Identity<string>(i => $"Test Product {i}");

        private readonly Value<string> _articleNumber;
        private readonly Value<string> _name;
        private readonly Value<decimal> _price;
        private readonly Value<string> _description;
        private readonly Values<string> _tags;

        public ProductBuilder()
        {
            _articleNumber = Value(p => p.ArticleNumber, ArticleNumberIdentity).Required();
            _name = Value(p => p.Name, NameIdentity).Required();
            _price = Value(p => p.Price, 41.99m).Required().Validate(p => p >= 0);
            _description = Value(p => p.Description, "This is an awesome product.");
            _tags = Values(p => p.Tags, i => $"Tag {i}");
        }

        public ProductBuilder WithArticleNumber(string articleNumber) => SetValue(_articleNumber, articleNumber);

        public ProductBuilder WithName(string name) => SetValue(_name, name);

        public ProductBuilder WithPrice(decimal price) => SetValue(_price, price);

        public ProductBuilder WithDescription(string description) => SetValue(_description, description);
        public ProductBuilder WithoutDescription() => NoValue(_description);

        public ProductBuilder WithTags(ValuesCustomizer<string> customizer) => CustomizeValues(_tags, customizer);

        protected override Product BuildInternal()
        {
            return new Product
            {
                ArticleNumber = _articleNumber,
                Name = _name,
                Price = _price,
                Description = _description,
                Tags = _tags,
            };
        }
    }
}
