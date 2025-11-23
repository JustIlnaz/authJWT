using Microsoft.AspNetCore.Mvc;

namespace authJWT.Interfaces
{
    public interface IUserService
    {
        Task<ActionResult> EditRole(int Id, int RoleId);
    }
}

