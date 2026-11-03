using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize(Policy = "Products")]
    public class ProductsController : ControllerBase
    {
        private readonly CatalogService service;

        public ProductsController(CatalogService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool activeOnly = false)
        {
            return Ok(await service.GetProductsAsync(activeOnly));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductRequest request)
        {
            return Ok(await service.CreateProductAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ProductRequest request)
        {
            await service.UpdateProductAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
