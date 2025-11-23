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
        public async Task<ActionResult> GetCategories([FromQuery] int? id)
        {
            if (id.HasValue)
            {
                var category = await _context.Categories
                    .Include(c => c.Items.Where(i => i.IsActive))
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

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

            return await _service.GetCategories();
        }

        [HttpPost]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> CreateCategory([FromQuery] string name)
        {
            return await _service.CreateCategory(name);
        }

        [HttpPut]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> UpdateCategory([FromQuery] int id, [FromQuery] string name)
        {
            if (id <= 0)
                return BadRequest(new { message = "Неверный ID категории" });

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Название категории не может быть пустым" });

            if (name.Length > 100)
                return BadRequest(new { message = "Название категории не может превышать 100 символов" });

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Категория не найдена" });

            if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != id))
                return BadRequest(new { message = "Категория с таким названием уже существует" });

            category.Name = name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно обновлено" });
        }

        [HttpDelete]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> DeleteCategory([FromQuery] int id)
        {
            return await _service.DeleteCategory(id);
        }
    }
}

