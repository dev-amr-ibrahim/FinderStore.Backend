using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.ValueObjects
{
    public class ShippingAddress
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string AddressLine1 { get; private set; }
        public string? AddressLine2 { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public string Country { get; private set; }
        public string Phone { get; private set; }
        public string? DeliveryInstructions { get; private set; }

        private ShippingAddress() { } // For EF Core

        public ShippingAddress(
            string firstName,
            string lastName,
            string addressLine1,
            string? addressLine2,
            string city,
            string state,
            string zipCode,
            string country,
            string phone,
            string? deliveryInstructions = null)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            AddressLine1 = addressLine1 ?? throw new ArgumentNullException(nameof(addressLine1));
            AddressLine2 = addressLine2;
            City = city ?? throw new ArgumentNullException(nameof(city));
            State = state ?? throw new ArgumentNullException(nameof(state));
            ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
            Country = country ?? throw new ArgumentNullException(nameof(country));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            DeliveryInstructions = deliveryInstructions;
        }

        public string GetFullAddress()
        {
            var address = $"{AddressLine1}";
            if (!string.IsNullOrEmpty(AddressLine2))
                address += $", {AddressLine2}";
            address += $", {City}, {State} {ZipCode}, {Country}";
            return address;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
