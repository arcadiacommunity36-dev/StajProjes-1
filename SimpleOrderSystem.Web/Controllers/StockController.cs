using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly StockService service;

        public StockController(StockService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Authorize(Policy = "Reports")]
        public async Task<IActionResult> Get()
        {
            return Ok(await service.GetAllAsync());
        }

        [HttpPost]
        [Authorize(Policy = "ManageStock")]
        public async Task<IActionResult> Create(StockRequest request)
        {
            return Ok(await service.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ManageStock")]
        public async Task<IActionResult> Update(int id, StockRequest request)
        {
            await service.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "ManageStock")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
    }
}
