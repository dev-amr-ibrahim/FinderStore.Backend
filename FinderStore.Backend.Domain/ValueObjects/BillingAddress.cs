using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Domain.ValueObjects
{
    public class BillingAddress
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
        public string? TaxId { get; private set; }
        public string? CompanyName { get; private set; }

        private BillingAddress() { } // For EF Core

        public BillingAddress(
            string firstName,
            string lastName,
            string addressLine1,
            string? addressLine2,
            string city,
            string state,
            string zipCode,
            string country,
            string phone,
            string? taxId = null,
            string? companyName = null)
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
            TaxId = taxId;
            CompanyName = companyName;
        }

        public string GetFullAddress()
        {
            var address = $"{AddressLine1}";
            if (!string.IsNullOrEmpty(AddressLine2))
                address += $", {AddressLine2}";
            if (!string.IsNullOrEmpty(CompanyName))
                address = $"{CompanyName}, {address}";
            address += $", {City}, {State} {ZipCode}, {Country}";
            return address;
        }
    }
}
