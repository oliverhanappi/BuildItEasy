using System;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ContactBuilder : Builder<Contact, ContactBuilder>
    {
        private readonly Order _order;
        
        private readonly Property<string> _firstName = RequiredProperty<string>("Max");
        private readonly Property<string> _lastName = RequiredProperty<string>("Mustermann");
        private readonly Property<string> _mailAddress = RequiredProperty<string>("max.mustermann@test.local");

        public ContactBuilder(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));
        }
        
        public ContactBuilder WithFirstName(string firstName) => SetValue(_firstName, firstName);
        public ContactBuilder WithLastName(string lastName) => SetValue(_lastName, lastName);
        public ContactBuilder WithMailAddress(string mailAddress) => SetValue(_mailAddress, mailAddress);
        
        public override Contact Build()
        {
            return new Contact(_order, _firstName, _lastName, _mailAddress);
        }
    }
}
