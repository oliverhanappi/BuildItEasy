using System;

namespace BuildItEasy.Tests.Sample.Domain
{
    public class Contact
    {
        public Order Order { get; }
        
        public string FirstName { get; }
        public string LastName { get; }

        public string MailAddress { get; }

        public Contact(Order order, string firstName, string lastName, string mailAddress)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            MailAddress = mailAddress ?? throw new ArgumentNullException(nameof(mailAddress));
        }
    }
}
