using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
