using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class CartService : ICartService
    {
        private readonly ContextDb _context;

        public CartService(ContextDb context)
        {
            _context = context;
        }

        public async Task<ActionResult> GetCart(int userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Category)
                .Where(c => c.UserId == userId && c.OrderId == null)
                .Select(c => new
                {
                    c.Id,
                    ItemId = c.ItemId,
                    ItemName = c.Items.Name,
                    ItemPrice = c.Items.Price,
                    CountItem = c.CountItem,
                    TotalPrice = c.Items.Price * c.CountItem
                })
                .ToListAsync();

            var total = cartItems.Sum(c => c.TotalPrice);

            return new OkObjectResult(new
            {
                items = cartItems,
                total
            });
        }

        public async Task<ActionResult> AddToCart(int userId, int itemId, decimal quantity)
        {
            var item = await _context.Items.FindAsync(itemId);

            if (item == null || !item.IsActive)
                return new NotFoundObjectResult(new { message = "Товар не найден" });

            if (item.Count < quantity)
                return new BadRequestObjectResult(new { message = "Недостаточно товара на складе" });

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ItemId == itemId && c.OrderId == null);

            if (existingCartItem != null)
            {
                existingCartItem.CountItem += quantity;
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = userId,
                    ItemId = itemId,
                    CountItem = quantity
                };
                _context.Carts.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Товар добавлен в корзину" });
        }

        public async Task<ActionResult> UpdateCartItem(int userId, int id, decimal quantity)
        {
            var cartItem = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.OrderId == null);

            if (cartItem == null)
                return new NotFoundObjectResult(new { message = "Товар в корзине не найден" });

            if (cartItem.Items.Count < quantity)
                return new BadRequestObjectResult(new { message = "Недостаточно товара на складе" });

            cartItem.CountItem = quantity;
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        public async Task<ActionResult> RemoveFromCart(int userId, int id)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.OrderId == null);

            if (cartItem == null)
                return new NotFoundObjectResult(new { message = "Товар в корзине не найден" });

            _context.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}

