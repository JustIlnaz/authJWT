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
    [AuthorizeRole("Покупатель")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult> GetCart()
        {
            return await _service.GetCart(GetUserId());
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            return await _service.AddToCart(GetUserId(), request.ItemId, request.Quantity);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCartItem(int id, [FromBody] decimal quantity)
        {
            return await _service.UpdateCartItem(GetUserId(), id, quantity);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveFromCart(int id)
        {
            return await _service.RemoveFromCart(GetUserId(), id);
        }
    }

   
}

