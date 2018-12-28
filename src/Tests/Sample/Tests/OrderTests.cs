using System;
using System.Linq;
using BuildItEasy.Tests.Sample.Builders;
using BuildItEasy.Tests.Sample.Domain;
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
                .WithOrderLines(ls => ls.WithExactly(
                    l => l.WithQuantity(2),
                    l => l.WithQuantity(1))
                )
                .Paid()
                .Build();

            var serializerSettings = new JsonSerializerSettings
                {PreserveReferencesHandling = PreserveReferencesHandling.Objects};
            var json = JsonConvert.SerializeObject(order, Formatting.Indented, serializerSettings);
            Console.WriteLine(json);
        }

        [Test]
        public void BuildsCanceledOrderWithCustomCancellationReasion()
        {
            var order = new OrderBuilder()
                .Canceled()
                .WithCancellationReason(OrderCancellationReason.ShipmentUndeliverable)
                .Build();

            Assert.That(order.History.Last().Message, Is.EqualTo("Canceled with reason ShipmentUndeliverable."));
        }
    }
}