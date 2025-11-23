using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using authJWT.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class ItemService : IItemService
    {
        private readonly ContextDb _context;

        public ItemService(ContextDb context)
        {
            _context = context;
        }

        public async Task<ActionResult> GetItems(int? categoryId, decimal? minPrice, decimal? maxPrice, bool? inStock, string? sortBy, string? sortOrder)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Where(i => i.IsActive);

            if (categoryId.HasValue)
                query = query.Where(i => i.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(i => i.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(i => i.Price <= maxPrice.Value);

            if (inStock.HasValue && inStock.Value)
                query = query.Where(i => i.Count > 0);

            query = sortBy?.ToLower() switch
            {
                "price" => sortOrder == "desc" ? query.OrderByDescending(i => i.Price) : query.OrderBy(i => i.Price),
                "date" => sortOrder == "desc" ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
                _ => query.OrderBy(i => i.Name)
            };

            var items = await query.Select(i => new
            {
                i.Id,
                i.Name,
                i.Description,
                i.Price,
                i.Count,
                i.IsActive,
                i.CreatedAt,
                CategoryName = i.Category.Name,
                CategoryId = i.CategoryId
            }).ToListAsync();

            return new OkObjectResult(items);
        }

        public async Task<ActionResult> GetItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return new NotFoundObjectResult(new { message = "Товар не найден" });

            return new OkObjectResult(new
            {
                item.Id,
                item.Name,
                item.Description,
                item.Price,
                item.Count,
                item.IsActive,
                item.CreatedAt,
                CategoryName = item.Category.Name,
                CategoryId = item.CategoryId
            });
        }

        public async Task<ActionResult> CreateItem(CreateItem request)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == request.Category);
            if (category == null)
            {
                category = new Category { Name = request.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var item = new Item
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Count = request.Count,
                CategoryId = category.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetItem), "Items", new { id = item.Id }, item);
        }

        public async Task<ActionResult> UpdateItem(int id, CreateItem request)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return new NotFoundObjectResult(new { message = "Товар не найден" });

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == request.Category);
            if (category == null)
            {
                category = new Category { Name = request.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            item.Name = request.Name;
            item.Description = request.Description;
            item.Price = request.Price;
            item.Count = request.Count;
            item.CategoryId = category.Id;

            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        public async Task<ActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return new NotFoundObjectResult(new { message = "Товар не найден" });

            item.IsActive = false;
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}

