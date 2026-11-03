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
    public class CatalogService
    {
        private readonly OrderDbContext db;

        public CatalogService(OrderDbContext db)
        {
            this.db = db;
        }

        public Task<List<CustomerView>> GetCustomersAsync(bool activeOnly = false)
        {
            return db.Customers.AsNoTracking().Where(x => !activeOnly || x.Status).OrderBy(x => x.Name)
                .Select(x => new CustomerView
                {
                    Id = x.Id, Name = x.Name, Code = x.Code, Address = x.Address,
                    PhoneNumber = x.PhoneNumber, Email = x.Email, Status = x.Status
                }).ToListAsync();
        }

        public async Task<CustomerView> CreateCustomerAsync(CustomerRequest request)
        {
            await CheckCustomerCodeAsync(request.Code, 0);
            var customer = new Customer();
            SetCustomer(customer, request);
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            return new CustomerView
            {
                Id = customer.Id, Name = customer.Name, Code = customer.Code, Address = customer.Address,
                PhoneNumber = customer.PhoneNumber, Email = customer.Email, Status = customer.Status
            };
        }

        public async Task UpdateCustomerAsync(int id, CustomerRequest request)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (customer == null)
                throw new BusinessException("The customer was not found.", 404);
            await CheckCustomerCodeAsync(request.Code, id);
            SetCustomer(customer, request);
            await db.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (customer == null)
                throw new BusinessException("The customer was not found.", 404);
            if (await db.Orders.AnyAsync(x => x.CustomerId == id))
                throw new BusinessException("Customers with orders cannot be deleted. Deactivate the customer instead.");
            db.Customers.Remove(customer);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public Task<List<ColorView>> GetColorsAsync()
        {
            return db.Colors.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new ColorView { Id = x.Id, Name = x.Name, Status = x.Status }).ToListAsync();
        }

        public async Task<ColorView> CreateColorAsync(ColorRequest request)
        {
            await CheckColorNameAsync(request.Name, 0);
            var color = new Color { Name = request.Name.Trim(), Status = request.Status };
            db.Colors.Add(color);
            await db.SaveChangesAsync();
            return new ColorView { Id = color.Id, Name = color.Name, Status = color.Status };
        }

        public async Task UpdateColorAsync(int id, ColorRequest request)
        {
            var color = await db.Colors.FirstOrDefaultAsync(x => x.Id == id);
            if (color == null)
                throw new BusinessException("The color was not found.", 404);
            await CheckColorNameAsync(request.Name, id);
            color.Name = request.Name.Trim();
            color.Status = request.Status;
            await db.SaveChangesAsync();
        }

        public async Task DeleteColorAsync(int id)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var color = await db.Colors.FirstOrDefaultAsync(x => x.Id == id);
            if (color == null)
                throw new BusinessException("The color was not found.", 404);
            if (await db.Products.AnyAsync(x => x.ColorId == id))
                throw new BusinessException("This color is used by a product and cannot be deleted.");
            db.Colors.Remove(color);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public Task<List<ProductView>> GetProductsAsync(bool activeOnly = false)
        {
            return (from product in db.Products.AsNoTracking()
                    join color in db.Colors.AsNoTracking() on product.ColorId equals color.Id
                    where !activeOnly || product.Status && color.Status
                    orderby product.Name, product.Size
                    select new ProductView
                    {
                        Id = product.Id, Name = product.Name, ColorId = product.ColorId,
                        ColorName = color.Name, Size = product.Size, Status = product.Status
                    }).ToListAsync();
        }

        public async Task<ProductView> CreateProductAsync(ProductRequest request)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var color = await CheckColorAsync(request.ColorId);
            var product = new Product
            {
                Name = request.Name.Trim(), ColorId = request.ColorId,
                Size = request.Size, Status = request.Status
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ProductView
            {
                Id = product.Id, Name = product.Name, ColorId = product.ColorId,
                ColorName = color.Name, Size = product.Size, Status = product.Status
            };
        }

        public async Task UpdateProductAsync(int id, ProductRequest request)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                throw new BusinessException("The product was not found.", 404);
            await CheckColorAsync(request.ColorId);
            if (await db.Orders.AnyAsync(x => x.ProductId == id) &&
                (product.ColorId != request.ColorId || product.Size != request.Size))
                throw new BusinessException("The color and size of an ordered product cannot be changed. Create a new product instead.");
            product.Name = request.Name.Trim();
            product.ColorId = request.ColorId;
            product.Size = request.Size;
            product.Status = request.Status;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                throw new BusinessException("The product was not found.", 404);
            if (await db.Orders.AnyAsync(x => x.ProductId == id))
                throw new BusinessException("Products with orders cannot be deleted. Deactivate the product instead.");
            if (await db.Stocks.AnyAsync(x => x.ProductId == id))
                throw new BusinessException("Delete the stock record before deleting this product.");
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task CheckCustomerCodeAsync(string code, int id)
        {
            var value = code.Trim().ToUpperInvariant();
            if (await db.Customers.AnyAsync(x => x.Id != id && x.Code.ToUpper() == value))
                throw new BusinessException("This customer code is already in use.", 409);
        }

        private async Task CheckColorNameAsync(string name, int id)
        {
            var value = name.Trim().ToUpperInvariant();
            if (await db.Colors.AnyAsync(x => x.Id != id && x.Name.ToUpper() == value))
                throw new BusinessException("A color with this name already exists.", 409);
        }

        private async Task<Color> CheckColorAsync(int id)
        {
            var color = await db.Colors.FirstOrDefaultAsync(x => x.Id == id && x.Status);
            if (color == null)
                throw new BusinessException("Select an active color.");
            return color;
        }

        private static void SetCustomer(Customer customer, CustomerRequest request)
        {
            customer.Name = request.Name.Trim();
            customer.Code = request.Code.Trim().ToUpperInvariant();
            customer.Address = request.Address.Trim();
            customer.PhoneNumber = request.PhoneNumber.Trim();
            customer.Email = request.Email.Trim().ToLowerInvariant();
            customer.Status = request.Status;
        }
    }
}
