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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        [HttpGet]
        public async Task<ActionResult> GetOrders([FromQuery] int? id)
        {
            if (id.HasValue)
            {
                return await _service.GetOrder(id.Value, GetUserId(), GetUserRole());
            }

            return await _service.GetOrders(GetUserId(), GetUserRole());
        }

        [HttpPost]
        [AuthorizeRole("Покупатель")]
        public async Task<ActionResult> CreateOrder([FromQuery] CreateOrderRequest request)
        {
            return await _service.CreateOrder(GetUserId(), request.ShippingMethodId);
        }

        [HttpPut("status")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> UpdateOrderStatus([FromQuery] int id, [FromQuery] string statusName)
        {
            return await _service.UpdateOrderStatus(id, statusName);
        }

        [HttpDelete]
        public async Task<ActionResult> CancelOrder([FromQuery] int id)
        {
            return await _service.CancelOrder(id, GetUserId(), GetUserRole());
        }
    }

    
}

