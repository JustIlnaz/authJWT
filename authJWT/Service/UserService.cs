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
            if (Id <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID пользователя" });

            if (RoleId <= 0)
                return new BadRequestObjectResult(new { message = "Неверный ID роли" });

            var user = await _context.Users.FindAsync(Id);
            if (user == null)
                return new NotFoundObjectResult(new { message = "Пользователь не найден" });

            var role = await _context.Roles.FindAsync(RoleId);
            if (role == null)
                return new NotFoundObjectResult(new { message = "Роль не найдена" });

            if (user.RoleId == RoleId)
                return new BadRequestObjectResult(new { message = "Пользователь уже имеет эту роль" });

            user.RoleId = RoleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Успешно обновлено" });
        }
    }
}

