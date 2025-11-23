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
            [FromQuery] int? id,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? inStock,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            if (id.HasValue)
            {
                return await _service.GetItem(id.Value);
            }
            
            return await _service.GetItems(categoryId, minPrice, maxPrice, inStock, sortBy, sortOrder);
        }

        [HttpPost]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> CreateItem([FromQuery] CreateItem request)
        {
            return await _service.CreateItem(request);
        }

        [HttpPut]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> UpdateItem([FromQuery] int id, [FromQuery] CreateItem request)
        {
            return await _service.UpdateItem(id, request);
        }

        [HttpDelete]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> DeleteItem([FromQuery] int id)
        {
            return await _service.DeleteItem(id);
        }
    }
}

