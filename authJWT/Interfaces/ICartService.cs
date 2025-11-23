using Microsoft.AspNetCore.Mvc;

namespace authJWT.Interfaces
{
    public interface ICartService
    {
        Task<ActionResult> GetCart(int userId);
        Task<ActionResult> AddToCart(int userId, int itemId, decimal quantity);
        Task<ActionResult> UpdateCartItem(int userId, int id, decimal quantity);
        Task<ActionResult> RemoveFromCart(int userId, int id);
    }
}

