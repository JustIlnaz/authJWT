using Microsoft.AspNetCore.Mvc;
using authJWT.Requests;

namespace authJWT.Interfaces
{
    public interface IItemService
    {
        Task<ActionResult> GetItems(int? categoryId, decimal? minPrice, decimal? maxPrice, bool? inStock, string? sortBy, string? sortOrder);
        Task<ActionResult> GetItem(int id);
        Task<ActionResult> CreateItem(CreateItem request);
        Task<ActionResult> UpdateItem(int id, CreateItem request);
        Task<ActionResult> DeleteItem(int id);
    }
}

