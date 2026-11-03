using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService service;

        public OrdersController(OrderService service)
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
        [Authorize(Policy = "CreateOrders")]
        public async Task<IActionResult> Create(OrderRequest request)
        {
            return Ok(await service.CreateAsync(request));
        }
    }
}
