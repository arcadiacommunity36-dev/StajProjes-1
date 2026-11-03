using System.Threading.Tasks;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOrderSystem.Web.Controllers
{
    [ApiController]
    [Route("api/colors")]
    [Authorize(Policy = "Products")]
    public class ColorsController : ControllerBase
    {
        private readonly CatalogService service;

        public ColorsController(CatalogService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await service.GetColorsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ColorRequest request)
        {
            return Ok(await service.CreateColorAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ColorRequest request)
        {
            await service.UpdateColorAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteColorAsync(id);
            return NoContent();
        }
    }
}
