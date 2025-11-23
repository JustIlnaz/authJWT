using Microsoft.AspNetCore.Mvc;

namespace authJWT.Interfaces
{
    public interface ICategoryService
    {
        Task<ActionResult> CreateCategory(string Name);
        Task<ActionResult> DeleteCategory(int Id);
        Task<ActionResult> GetCategories();
    }
}

