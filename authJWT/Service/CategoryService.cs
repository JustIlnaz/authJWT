using authJWT.Connection;
using authJWT.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ContextDb _context;

        public CategoryService(ContextDb context)
        {
            _context = context;
        }

        public async Task<ActionResult> CreateCategory(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return new BadRequestObjectResult(new { message = "Название категории не может быть пустым" });

            if (Name.Length > 100)
                return new BadRequestObjectResult(new { message = "Название категории не может превышать 100 символов" });

            if (await _context.Categories.AnyAsync(c => c.Name == Name))
                return new BadRequestObjectResult(new { message = "Категория с таким названием уже существует" });

            var category = new Models.Category { Name = Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно создано", data = category });
        }

        public async Task<ActionResult> DeleteCategory(int Id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (category == null)
                return new NotFoundObjectResult(new { message = "Категория не найдена" });

            if (category.Items.Any())
                return new BadRequestObjectResult(new { message = "Невозможно удалить категорию, в которой есть товары" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно удалено" });
        }

        public async Task<ActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    ItemsCount = c.Items.Count(i => i.IsActive)
                })
                .ToListAsync();

            return new OkObjectResult(categories);
        }
    }
}

