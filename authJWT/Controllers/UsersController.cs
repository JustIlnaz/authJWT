using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using authJWT.Requests;
using authJWT.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ContextDb _context;
        private readonly IUserService _userService;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(ContextDb context, IUserService userService)
        {
            _context = context;
            _userService = userService;
            _passwordHasher = new PasswordHasher<User>();
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        [HttpGet]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> GetUsers([FromQuery] int? id, [FromQuery] string? role)
        {
            if (id.HasValue)
            {
                var userRole = GetUserRole();
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id.Value);

                if (user == null)
                    return NotFound(new { message = "Пользователь не найден" });

                if (userRole == "Менеджер" && user.Role.NameRole != "Покупатель")
                    return Unauthorized(new { message = "Недостаточно прав для выполнения этой операции" });

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.Phone,
                    user.AdressDelivery,
                    user.CreatedAt,
                    user.UpdatedAt,
                    Role = user.Role.NameRole
                });
            }

            IQueryable<User> query = _context.Users.Include(u => u.Role);

            var currentUserRole = GetUserRole();
            if (currentUserRole == "Менеджер")
            {
                query = query.Where(u => u.Role.NameRole == "Покупатель");
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role.NameRole == role);
            }

            var users = await query.Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.Phone,
                u.CreatedAt,
                Role = u.Role.NameRole
            }).ToListAsync();

            return Ok(users);
        }

        [HttpPost("employees")]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> CreateEmployee([FromQuery] CreateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email не может быть пустым" });

            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { message = "Неверный формат email" });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Пароль не может быть пустым" });

            if (request.Password.Length < 6)
                return BadRequest(new { message = "Пароль должен содержать минимум 6 символов" });

            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest(new { message = "ФИО не может быть пустым" });

            if (string.IsNullOrWhiteSpace(request.Phone))
                return BadRequest(new { message = "Телефон не может быть пустым" });

            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { message = "Роль не может быть пустой" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Пользователь с таким email уже существует" });

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NameRole == request.Role);
            if (role == null)
                return NotFound(new { message = "Роль не найдена" });

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                RoleId = role.IdRole,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно создано", data = new
            {
                user.Id,
                user.Email,
                user.FullName,
                Role = role.NameRole
            }});
        }

        [HttpPut("role")]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> EditRole([FromQuery] int id, [FromQuery] int roleId)
        {
            return await _userService.EditRole(id, roleId);
        }

        [HttpPut]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<IActionResult> UpdateUser([FromQuery] int id, [FromQuery] UpdateUserRequest request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Неверный ID пользователя" });

            var userRole = GetUserRole();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            if (userRole == "Менеджер" && user.Role.NameRole != "Покупатель")
                return Unauthorized(new { message = "Недостаточно прав для выполнения этой операции" });

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest(new { message = "Неверный формат email" });

                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest(new { message = "Пользователь с таким email уже существует" });
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;

            if (!string.IsNullOrEmpty(request.AdressDelivery))
                user.AdressDelivery = request.AdressDelivery;

            if (userRole == "Администратор" && !string.IsNullOrEmpty(request.Role))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.NameRole == request.Role);
                if (role != null)
                    user.RoleId = role.IdRole;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно обновлено" });
        }

        [HttpDelete]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Неверный ID пользователя" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            // нельзя удалить пользователя у которого есть активные заказы
            var hasActiveOrders = await _context.Orders
                .Include(o => o.Carts)
                .AnyAsync(o => o.Carts.Any(c => c.UserId == id) && o.Status.Name != "cancelled" && o.Status.Name != "delivered");

            if (hasActiveOrders)
                return BadRequest(new { message = "Невозможно удалить пользователя с активными заказами" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно удалено" });
        }
    }

 

   
}




