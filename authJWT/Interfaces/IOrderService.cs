using Microsoft.AspNetCore.Mvc;

namespace authJWT.Interfaces
{
    public interface IOrderService
    {
        Task<ActionResult> GetOrders(int? userId, string? userRole);
        Task<ActionResult> GetOrder(int id, int? userId, string? userRole);
        Task<ActionResult> CreateOrder(int userId, int shippingMethodId);
        Task<ActionResult> UpdateOrderStatus(int id, string statusName);
        Task<ActionResult> CancelOrder(int id, int? userId, string? userRole);
    }
}

