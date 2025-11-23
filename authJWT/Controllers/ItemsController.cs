using authJWT.Interfaces;
using authJWT.Requests;
using authJWT.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;

        public ItemsController(IItemService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult> GetItems(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? inStock,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            return await _service.GetItems(categoryId, minPrice, maxPrice, inStock, sortBy, sortOrder);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetItem(int id)
        {
            return await _service.GetItem(id);
        }

        [HttpPost]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> CreateItem([FromBody] CreateItem request)
        {
            return await _service.CreateItem(request);
        }

        [HttpPut("{id}")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> UpdateItem(int id, [FromBody] CreateItem request)
        {
            return await _service.UpdateItem(id, request);
        }

        [HttpDelete("{id}")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            return await _service.DeleteItem(id);
        }
    }
}

