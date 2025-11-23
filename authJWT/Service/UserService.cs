using authJWT.Connection;
using authJWT.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class UserService : IUserService
    {
        private readonly ContextDb _context;

        public UserService(ContextDb context)
        {
            _context = context;
        }

        public async Task<ActionResult> EditRole(int Id, int RoleId)
        {
            var user = await _context.Users.FindAsync(Id);
            if (user == null)
                return new NotFoundObjectResult(new { message = "Пользователь не найден" });

            var role = await _context.Roles.FindAsync(RoleId);
            if (role == null)
                return new NotFoundObjectResult(new { message = "Роль не найдена" });

            user.RoleId = RoleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}

