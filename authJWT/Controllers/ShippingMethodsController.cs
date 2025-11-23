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
        public async Task<ActionResult> CreateShippingMethod([FromBody] CreateShippingMethodRequest request)
        {
            var method = new ShippingMethod
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            _context.ShippingMethods.Add(method);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShippingMethods), new { id = method.Id }, method);
        }

        [HttpPut("{id}")]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> UpdateShippingMethod(int id, [FromBody] CreateShippingMethodRequest request)
        {
            var method = await _context.ShippingMethods.FindAsync(id);
            if (method == null)
                return NotFound(new { message = "Способ доставки не найден" });

            method.Name = request.Name;
            method.Description = request.Description;
            method.Price = request.Price;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> DeleteShippingMethod(int id)
        {
            var method = await _context.ShippingMethods
                .Include(sm => sm.Orders)
                .FirstOrDefaultAsync(sm => sm.Id == id);

            if (method == null)
                return NotFound(new { message = "Способ доставки не найден" });

            if (method.Orders.Any())
                return BadRequest(new { message = "Невозможно удалить способ доставки, который используется в заказах" });

            _context.ShippingMethods.Remove(method);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

   
}

