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
    public class StockService
    {
        private readonly OrderDbContext db;

        public StockService(OrderDbContext db)
        {
            this.db = db;
        }

        public async Task<List<StockView>> GetAllAsync()
        {
            var rows = await (from stock in db.Stocks.AsNoTracking()
                              join product in db.Products.AsNoTracking() on stock.ProductId equals product.Id
                              join color in db.Colors.AsNoTracking() on product.ColorId equals color.Id
                              orderby product.Name, product.Size
                              select new { Stock = stock, Product = product, Color = color }).ToListAsync();
            return rows.Select(x => new StockView
            {
                Id = x.Stock.Id, ProductId = x.Product.Id, ProductName = x.Product.Name,
                ColorName = x.Color.Name, Size = x.Product.Size, Quantity = x.Stock.Quantity,
                IsReadyForSale = x.Stock.IsReadyForSale, Status = x.Stock.Status,
                CanSell = x.Stock.Status && x.Stock.IsReadyForSale && x.Stock.Quantity > 0 && x.Product.Status && x.Color.Status,
                RowVersion = x.Stock.RowVersion == null ? null : Convert.ToBase64String(x.Stock.RowVersion)
            }).ToList();
        }

        public async Task<StockView> CreateAsync(StockRequest request)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await CheckProductAsync(request.ProductId);
            await CheckDuplicateAsync(request.ProductId, 0);
            var stock = new Stock
            {
                ProductId = request.ProductId, Quantity = request.Quantity,
                IsReadyForSale = request.IsReadyForSale, Status = request.Status
            };
            db.Stocks.Add(stock);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (await GetAllAsync()).First(x => x.Id == stock.Id);
        }

        public async Task UpdateAsync(int id, StockRequest request)
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            if (stock == null)
                throw new BusinessException("The stock record was not found.", 404);
            if (stock.ProductId != request.ProductId)
                throw new BusinessException("A stock record's product cannot be changed. Create a separate stock record instead.");
            await CheckProductAsync(request.ProductId);
            byte[] version;
            try
            {
                version = Convert.FromBase64String(request.RowVersion ?? "");
            }
            catch (FormatException)
            {
                throw new BusinessException("Refresh the stock report before editing this record.");
            }
            if (version.Length != 8)
                throw new BusinessException("Refresh the stock report before editing this record.");
            db.Entry(stock).Property(x => x.RowVersion).OriginalValue = version;
            stock.Quantity = request.Quantity;
            stock.IsReadyForSale = request.IsReadyForSale;
            stock.Status = request.Status;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("Stock has changed. Refresh the report and try again.", 409);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            if (stock == null)
                throw new BusinessException("The stock record was not found.", 404);
            db.Stocks.Remove(stock);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException("Stock has changed. Refresh the report and try again.", 409);
            }
        }

        private async Task CheckProductAsync(int id)
        {
            if (!await db.Products.AnyAsync(x => x.Id == id && x.Status))
                throw new BusinessException("Select an active product.");
        }

        private async Task CheckDuplicateAsync(int productId, int id)
        {
            if (await db.Stocks.AnyAsync(x => x.ProductId == productId && x.Id != id))
                throw new BusinessException("This product already has a stock record. Edit that record instead.", 409);
        }
    }
}
