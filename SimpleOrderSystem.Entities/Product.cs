using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public int ColorId { get; set; }
        public Size Size { get; set; }
    }
}
