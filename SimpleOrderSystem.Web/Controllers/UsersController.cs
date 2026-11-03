using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = "Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService service;

        public UsersController(UserService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserRequest request)
        {
            return Ok(await service.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UserRequest request)
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
