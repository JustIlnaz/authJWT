using authJWT.Connection;
using authJWT.Models;
using authJWT.Requests;
using authJWT.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    public class ShippingMethodsController : ControllerBase
    {
        private readonly ContextDb _context;

        public ShippingMethodsController(ContextDb context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetShippingMethods()
        {
            var methods = await _context.ShippingMethods
                .Select(sm => new
                {
                    sm.Id,
                    sm.Name,
                    sm.Description,
                    sm.Price
                })
                .ToListAsync();

            return Ok(methods);
        }

        [HttpPost]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> CreateShippingMethod([FromQuery] CreateShippingMethodRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Название способа доставки не может быть пустым" });

            if (request.Name.Length > 100)
                return BadRequest(new { message = "Название способа доставки не может превышать 100 символов" });

            if (request.Price < 0)
                return BadRequest(new { message = "Цена доставки не может быть отрицательной" });

            if (await _context.ShippingMethods.AnyAsync(sm => sm.Name == request.Name))
                return BadRequest(new { message = "Способ доставки с таким названием уже существует" });

            var method = new ShippingMethod
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Price = request.Price
            };

            _context.ShippingMethods.Add(method);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно создано", data = method });
        }

        [HttpPut]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> UpdateShippingMethod([FromQuery] int id, [FromQuery] CreateShippingMethodRequest request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Неверный ID способа доставки" });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Название способа доставки не может быть пустым" });

            if (request.Price < 0)
                return BadRequest(new { message = "Цена доставки не может быть отрицательной" });

            var method = await _context.ShippingMethods.FindAsync(id);
            if (method == null)
                return NotFound(new { message = "Способ доставки не найден" });

            if (await _context.ShippingMethods.AnyAsync(sm => sm.Name == request.Name && sm.Id != id))
                return BadRequest(new { message = "Способ доставки с таким названием уже существует" });

            method.Name = request.Name;
            method.Description = request.Description ?? string.Empty;
            method.Price = request.Price;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно обновлено" });
        }

        [HttpDelete]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> DeleteShippingMethod([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Неверный ID способа доставки" });

            var method = await _context.ShippingMethods
                .Include(sm => sm.Orders)
                .FirstOrDefaultAsync(sm => sm.Id == id);

            if (method == null)
                return NotFound(new { message = "Способ доставки не найден" });

            if (method.Orders.Any())
                return BadRequest(new { message = "Невозможно удалить способ доставки, который используется в заказах" });

            _context.ShippingMethods.Remove(method);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно удалено" });
        }
    }

   
}

