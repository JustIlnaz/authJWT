using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class OrderService : IOrderService
    {
        private readonly ContextDb _context;

        public OrderService(ContextDb context)
        {
            _context = context;
        }

        public async Task<ActionResult> GetOrders(int? userId, string? userRole)
        {
            if (userId.HasValue && userId.Value <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID пользователя" });

            IQueryable<Order> query = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.ShippingMethod)
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items);

            if (userRole == "Покупатель" && userId.HasValue)
            {
                query = query.Where(o => o.Carts.Any(c => c.UserId == userId.Value));
            }

            var orders = await query.Select(o => new
            {
                o.Id,
                Status = o.Status.Name,
                DateOrder = o.DateOrder,
                ShippingMethod = o.ShippingMethod.Name,
                ShippingPrice = o.ShippingMethod.Price,
                Items = o.Carts.Select(c => new
                {
                    ItemName = c.Items.Name,
                    Quantity = c.CountItem,
                    Price = c.Items.Price
                }),
                Total = o.Carts.Sum(c => c.Items.Price * c.CountItem) + o.ShippingMethod.Price
            }).ToListAsync();

            return new OkObjectResult(orders);
        }

        public async Task<ActionResult> GetOrder(int id, int? userId, string? userRole)
        {
            if (id <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID заказа" });

            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.ShippingMethod)
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items)
                .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return new NotFoundObjectResult(new { message = "Заказ не найден" });

            if (userRole == "Покупатель" && userId.HasValue && !order.Carts.Any(c => c.UserId == userId.Value))
                return new UnauthorizedObjectResult(new { message = "Недостаточно прав для выполнения этой операции" });

            return new OkObjectResult(new
            {
                order.Id,
                Status = order.Status.Name,
                DateOrder = order.DateOrder,
                ShippingMethod = order.ShippingMethod.Name,
                ShippingPrice = order.ShippingMethod.Price,
                Items = order.Carts.Select(c => new
                {
                    ItemName = c.Items.Name,
                    Quantity = c.CountItem,
                    Price = c.Items.Price,
                    Total = c.Items.Price * c.CountItem
                }),
                Total = order.Carts.Sum(c => c.Items.Price * c.CountItem) + order.ShippingMethod.Price
            });
        }

        public async Task<ActionResult> CreateOrder(int userId, int shippingMethodId)
        {
            if (userId <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID пользователя" });

            if (shippingMethodId <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID способа доставки" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new NotFoundObjectResult(new { message = "Пользователь не найден" });

            var cartItems = await _context.Carts
                .Include(c => c.Items)
                .Where(c => c.UserId == userId && c.OrderId == null)
                .ToListAsync();

            if (!cartItems.Any())
                return new BadRequestObjectResult(new { message = "Корзина пуста" });

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Items.Count < cartItem.CountItem)
                    return new BadRequestObjectResult(new { message = $"Недостаточно товара '{cartItem.Items.Name}' на складе" });
            }

            var status = await _context.Statuses.FirstOrDefaultAsync(s => s.Name == "pending");
            if (status == null)
            {
                status = new Status { Name = "pending" };
                _context.Statuses.Add(status);
                await _context.SaveChangesAsync();
            }

            var shippingMethod = await _context.ShippingMethods.FindAsync(shippingMethodId);
            if (shippingMethod == null)
                return new NotFoundObjectResult(new { message = "Способ доставки не найден" });

            var order = new Order
            {
                StatusId = status.Id,
                ShippingMethodId = shippingMethodId,
                DateOrder = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cartItems)
            {
                cartItem.OrderId = order.Id;
                cartItem.Items.Count -= cartItem.CountItem;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ItemId = cartItem.ItemId,
                    Quantity = (int)cartItem.CountItem,
                    UnitPrice = cartItem.Items.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно создано", data = order });
        }

        public async Task<ActionResult> UpdateOrderStatus(int id, string statusName)
        {
            if (id <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID заказа" });

            if (string.IsNullOrWhiteSpace(statusName))
                return new BadRequestObjectResult(new { message = "Название статуса не может быть пустым" });

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return new NotFoundObjectResult(new { message = "Заказ не найден" });

            var status = await _context.Statuses.FirstOrDefaultAsync(s => s.Name == statusName);
            if (status == null)
                return new NotFoundObjectResult(new { message = "Статус не найден" });

            if (statusName == "pending" && order.StatusId != 0)
            {
                var currentStatus = await _context.Statuses.FindAsync(order.StatusId);
                if (currentStatus != null && currentStatus.Name != "pending" && currentStatus.Name != "cancelled")
                    return new BadRequestObjectResult(new { message = "Нельзя вернуть заказ в статус 'pending' после обработки" });
            }

            order.StatusId = status.Id;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно обновлено" });
        }

        public async Task<ActionResult> CancelOrder(int id, int? userId, string? userRole)
        {
            if (id <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID заказа" });

            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return new NotFoundObjectResult(new { message = "Заказ не найден" });

            if (userRole == "Покупатель" && userId.HasValue)
            {
                if (!order.Carts.Any(c => c.UserId == userId.Value))
                    return new UnauthorizedObjectResult(new { message = "Недостаточно прав для выполнения этой операции" });

                if (order.Status.Name != "pending")
                    return new BadRequestObjectResult(new { message = "Можно отменить только заказ со статусом 'pending'" });
            }

            foreach (var cartItem in order.Carts)
            {
                cartItem.Items.Count += cartItem.CountItem;
            }

            var cancelledStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.Name == "cancelled");
            if (cancelledStatus == null)
            {
                cancelledStatus = new Status { Name = "cancelled" };
                _context.Statuses.Add(cancelledStatus);
                await _context.SaveChangesAsync();
            }

            order.StatusId = cancelledStatus.Id;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно удалено" });
        }
    }
}

