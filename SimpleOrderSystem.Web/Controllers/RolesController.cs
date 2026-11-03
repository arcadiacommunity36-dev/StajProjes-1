using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Policy = "Roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService service;

        public RolesController(RoleService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleRequest request)
        {
            return Ok(await service.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, RoleRequest request)
        {
            await service.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
    }
}
