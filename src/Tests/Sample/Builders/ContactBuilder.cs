using System;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ContactBuilder : Builder<Contact, ContactBuilder>
    {
        private readonly Order _order;
        
        private readonly Property<string> _firstName;
        private readonly Property<string> _lastName;
        private readonly Property<string> _mailAddress;

        public ContactBuilder(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));

            _firstName = Property(c => c.FirstName, "Max").Required();
            _lastName = Property(c => c.LastName, "Mustermann").Required();
            _mailAddress = Property(c => c.MailAddress, "max.mustermann@test.local").Required();
        }
        
        public ContactBuilder WithFirstName(string firstName) => SetValue(_firstName, firstName);
        public ContactBuilder WithLastName(string lastName) => SetValue(_lastName, lastName);
        public ContactBuilder WithMailAddress(string mailAddress) => SetValue(_mailAddress, mailAddress);

        protected override Contact BuildInternal()
        {
            return new Contact(_order, _firstName, _lastName, _mailAddress);
        }
    }
}
