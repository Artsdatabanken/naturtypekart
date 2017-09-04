using Newtonsoft.Json;

namespace Nin.Types
{
    public abstract class ContactBase
    {
        public ContactBase() {}
        public ContactBase(ContactBase contact)
        {
            Company = contact.Company;
            ContactPerson = contact.ContactPerson;
            Email = contact.Email;
            Phone = contact.Phone;
            Homesite = contact.Homesite;
        }

        public string Company { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Homesite { get; set; }
    }
}

namespace Nin.Types.RavenDb
{
    public class Contact : ContactBase
    {
        public Contact() {}
        public Contact(MsSql.Contact contact) : base (contact) {}
    }
}

namespace Nin.Types.MsSql
{
    public class Contact : ContactBase
    {
        public Contact() {}
        public Contact(RavenDb.Contact contact) : base(contact) {}

        [JsonIgnore]
        public int Id { get; set; }
    }
}
