using System;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class ContactBuilder : Builder<Contact, ContactBuilder>
    {
        private readonly Order _order;
        
        private readonly Value<string> _firstName;
        private readonly Value<string> _lastName;
        private readonly Value<string> _mailAddress;

        public ContactBuilder(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));

            _firstName = Value(c => c.FirstName, "Max").Required();
            _lastName = Value(c => c.LastName, "Mustermann").Required();
            _mailAddress = Value(c => c.MailAddress, "max.mustermann@test.local").Required();
        }
        
        public ContactBuilder WithFirstName(string firstName) => SetValue(_firstName, firstName);
        public ContactBuilder WithLastName(string lastName) => SetValue(_lastName, lastName);
        public ContactBuilder WithMailAddress(string mailAddress) => SetValue(_mailAddress, mailAddress);

        protected override Contact BuildInternal()
        {
            var contact = new Contact(_order, _firstName, _lastName, _mailAddress);
            _order.SetContact(contact);

            return contact;
        }
    }
}
