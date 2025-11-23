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
        public async Task<ActionResult> GetOrders()
        {
            return await _service.GetOrders(GetUserId(), GetUserRole());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrder(int id)
        {
            return await _service.GetOrder(id, GetUserId(), GetUserRole());
        }

        [HttpPost]
        [AuthorizeRole("Покупатель")]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            return await _service.CreateOrder(GetUserId(), request.ShippingMethodId);
        }

        [HttpPut("{id}/status")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string statusName)
        {
            return await _service.UpdateOrderStatus(id, statusName);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            return await _service.CancelOrder(id, GetUserId(), GetUserRole());
        }
    }

    
}

