using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using authJWT.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ContextDb _context;

        public CategoriesController(ICategoryService service, ContextDb context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            return await _service.GetCategories();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items.Where(i => i.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { message = "Категория не найдена" });

            return Ok(new
            {
                category.Id,
                category.Name,
                Items = category.Items.Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Price,
                    i.Count
                })
            });
        }

        [HttpPost]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> CreateCategory([FromBody] string name)
        {
            return await _service.CreateCategory(name);
        }

        [HttpPut("{id}")]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] string name)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Категория не найдена" });

            category.Name = name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            return await _service.DeleteCategory(id);
        }
    }
}

