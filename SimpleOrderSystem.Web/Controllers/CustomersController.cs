using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize(Policy = "Customers")]
    public class CustomersController : ControllerBase
    {
        private readonly CatalogService service;

        public CustomersController(CatalogService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool activeOnly = false)
        {
            return Ok(await service.GetCustomersAsync(activeOnly));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerRequest request)
        {
            return Ok(await service.CreateCustomerAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CustomerRequest request)
        {
            await service.UpdateCustomerAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}
