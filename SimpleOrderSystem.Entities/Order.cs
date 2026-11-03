using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities
{
    public class Order : BaseEntity
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
