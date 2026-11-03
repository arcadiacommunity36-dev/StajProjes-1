using System;
using SimpleOrderSystem.Entities;

namespace SimpleOrderSystem.Entities.Models
{
    public class UserView
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public int[] RoleIds { get; set; }
        public string[] Roles { get; set; }
        public string[] Permissions { get; set; }
        public bool Status { get; set; }
    }

    public class RoleView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
    }

    public class CustomerView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
    }

    public class ColorView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
    }

    public class ProductView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public Size Size { get; set; }
        public bool Status { get; set; }
    }

    public class StockView
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public Size Size { get; set; }
        public int Quantity { get; set; }
        public bool IsReadyForSale { get; set; }
        public bool CanSell { get; set; }
        public bool Status { get; set; }
        public string RowVersion { get; set; }
    }

    public class OrderView
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public Size Size { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
