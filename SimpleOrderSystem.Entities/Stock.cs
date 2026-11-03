using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities
{
    public class Stock : BaseEntity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool IsReadyForSale { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
