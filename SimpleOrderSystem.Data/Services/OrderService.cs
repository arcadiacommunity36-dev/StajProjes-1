using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SimpleOrderSystem.Data;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace SimpleOrderSystem.Data.Services
{
    public class OrderService
    {
        private readonly OrderDbContext db;
        private readonly ICurrentUser currentUser;

        public OrderService(OrderDbContext db, ICurrentUser currentUser)
        {
            this.db = db;
            this.currentUser = currentUser;
        }

        public Task<List<OrderView>> GetAllAsync()
        {
            return (from order in db.Orders.AsNoTracking()
                    join customer in db.Customers.AsNoTracking() on order.CustomerId equals customer.Id
                    join product in db.Products.AsNoTracking() on order.ProductId equals product.Id
                    join color in db.Colors.AsNoTracking() on product.ColorId equals color.Id
                    orderby order.CreatedDate descending, order.Id descending
                    select new OrderView
                    {
                        Id = order.Id, CustomerId = customer.Id, CustomerName = customer.Name,
                        ProductId = product.Id, ProductName = product.Name, ColorName = color.Name,
                        Size = product.Size, Quantity = order.Quantity, CreatedDate = order.CreatedDate
                    }).ToListAsync();
        }

        public async Task<OrderView> CreateAsync(OrderRequest request)
        {
            if (request.Quantity <= 0)
                throw new BusinessException("Quantity must be a positive whole number.");
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var customer = await db.Customers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.Status);
            if (customer == null)
                throw new BusinessException("Select an active customer.");

            var product = await (from item in db.Products.AsNoTracking()
                                 join color in db.Colors.AsNoTracking() on item.ColorId equals color.Id
                                 where item.Id == request.ProductId && item.Status && color.Status
                                 select new { Product = item, ColorName = color.Name }).FirstOrDefaultAsync();
            if (product == null)
                throw new BusinessException("The selected product is not available for sale.");

            var userId = currentUser.UserId;
            if (userId <= 0)
                throw new BusinessException("Sign in before creating an order.", 401);
            var date = DateTime.UtcNow;
            var updated = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE [Stocks]
SET [Quantity] = [Quantity] - {request.Quantity},
    [LastUpdatedUserId] = {userId}, [LastUpdatedDate] = {date}
WHERE [ProductId] = {request.ProductId}
  AND [Quantity] >= {request.Quantity}
  AND [IsReadyForSale] = 1 AND [Status] = 1 AND [IsDeleted] = 0");
            if (updated != 1)
                throw new BusinessException("The product is not ready for sale or there is not enough stock.");

            var order = new Order
            {
                CustomerId = request.CustomerId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OrderView
            {
                Id = order.Id, CustomerId = customer.Id, CustomerName = customer.Name,
                ProductId = product.Product.Id, ProductName = product.Product.Name,
                ColorName = product.ColorName, Size = product.Product.Size,
                Quantity = order.Quantity, CreatedDate = order.CreatedDate
            };
        }
    }
}
