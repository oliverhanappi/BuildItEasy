using System;
using BuildItEasy.Tests.Sample.Builders;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BuildItEasy.Tests.Sample.Tests
{
    [TestFixture]
    public class OrderTests
    {
        [Test]
        public void BuildOrder()
        {
            var order = new OrderBuilder()
                .WithContact(c => c.WithFirstName("Oliver"))
                .Paid()
                .Build();

            var serializerSettings = new JsonSerializerSettings {PreserveReferencesHandling = PreserveReferencesHandling.Objects};
            var json = JsonConvert.SerializeObject(order, Formatting.Indented, serializerSettings);
            Console.WriteLine(json);
        }
    }
}
