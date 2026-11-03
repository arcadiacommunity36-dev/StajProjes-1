using System.ComponentModel.DataAnnotations;
using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities.Models
{
    public class UserRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }
        [Required, StringLength(30)]
        public string PhoneNumber { get; set; }
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }
        [Required, RegularExpression("^(Male|Female|Other)$")]
        public string Gender { get; set; }
        [StringLength(128)]
        public string Password { get; set; }
        [Required, MinLength(1)]
        public int[] RoleIds { get; set; }
        public bool Status { get; set; } = true;
    }

    public class RoleRequest
    {
        [Required, StringLength(60)]
        public string Name { get; set; }
        public bool Status { get; set; } = true;
    }

    public class CustomerRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; }
        [Required, StringLength(30)]
        public string Code { get; set; }
        [Required, StringLength(500)]
        public string Address { get; set; }
        [Required, StringLength(30)]
        public string PhoneNumber { get; set; }
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }
        public bool Status { get; set; } = true;
    }

    public class ColorRequest
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public bool Status { get; set; } = true;
    }

    public class ProductRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; }
        [Range(1, int.MaxValue)]
        public int ColorId { get; set; }
        [EnumDataType(typeof(Size))]
        public Size Size { get; set; }
        public bool Status { get; set; } = true;
    }

    public class StockRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public bool IsReadyForSale { get; set; }
        public bool Status { get; set; } = true;
        public string RowVersion { get; set; }
    }

    public class OrderRequest
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
