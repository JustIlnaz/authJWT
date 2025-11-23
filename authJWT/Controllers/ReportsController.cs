using authJWT.Connection;
using authJWT.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    [AuthorizeRole("Администратор", "Менеджер")]
    public class ReportsController : ControllerBase
    {
        private readonly ContextDb _context;

        public ReportsController(ContextDb context)
        {
            _context = context;
        }

        // отчет по продажам
        [HttpGet("sales")]
        public async Task<ActionResult> GetSalesReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string period = "daily") // daily, weekly
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            if (period != "daily" && period != "weekly")
                return BadRequest(new { message = "Период должен быть 'daily' или 'weekly'" });

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start > end)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            var orders = await _context.Orders
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items)
                .Include(o => o.ShippingMethod)
                .Where(o => o.DateOrder >= start && o.DateOrder <= end && o.Status.Name != "cancelled")
                .ToListAsync();

            var totalRevenue = orders.Sum(o => 
                o.Carts.Sum(c => c.Items.Price * c.CountItem) + o.ShippingMethod.Price);

            var totalOrders = orders.Count;

            var report = new
            {
                Period = period,
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0
            };

            return Ok(report);
        }

        // топ 10 самых продаваемых товаров
        [HttpGet("top-items")]
        public async Task<ActionResult> GetTopItems(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int top = 10)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            if (top <= 0 || top > 100)
                return BadRequest(new { message = "Количество товаров должно быть от 1 до 100" });

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start > end)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            var topItems = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Order)
                .ThenInclude(o => o.Status)
                .Where(oi => oi.Order.DateOrder >= start && 
                            oi.Order.DateOrder <= end && 
                            oi.Order.Status.Name != "cancelled")
                .GroupBy(oi => new { oi.ItemId, oi.Item.Name })
                .Select(g => new
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(top)
                .ToListAsync();

            return Ok(topItems);
        }

        // общая выручка за период
        [HttpGet("revenue")]
        public async Task<ActionResult> GetRevenue(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start > end)
                return BadRequest(new { message = "Дата начала не может быть больше даты окончания" });

            var orders = await _context.Orders
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items)
                .Include(o => o.ShippingMethod)
                .Include(o => o.Status)
                .Where(o => o.DateOrder >= start && o.DateOrder <= end && o.Status.Name != "cancelled")
                .ToListAsync();

            var revenue = orders.Sum(o => 
                o.Carts.Sum(c => c.Items.Price * c.CountItem) + o.ShippingMethod.Price);

            var cancelledRevenue = await _context.Orders
                .Include(o => o.Carts)
                .ThenInclude(c => c.Items)
                .Include(o => o.ShippingMethod)
                .Include(o => o.Status)
                .Where(o => o.DateOrder >= start && o.DateOrder <= end && o.Status.Name == "cancelled")
                .ToListAsync();

            var cancelledTotal = cancelledRevenue.Sum(o => 
                o.Carts.Sum(c => c.Items.Price * c.CountItem) + o.ShippingMethod.Price);

            return Ok(new
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = revenue,
                CancelledOrdersRevenue = cancelledTotal,
                NetRevenue = revenue
            });
        }
    }
}

