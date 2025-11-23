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

        // GET: api/Users - Просмотр пользователей (Админ, Менеджер)
        [HttpGet]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> GetUsers([FromQuery] string? role)
        {
            IQueryable<User> query = _context.Users.Include(u => u.Role);

            var userRole = GetUserRole();
            if (userRole == "Менеджер")
            {
                // Менеджер видит только покупателей
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

        // GET: api/Users/5
        [HttpGet("{id}")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<ActionResult> GetUser(int id)
        {
            var userRole = GetUserRole();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            if (userRole == "Менеджер" && user.Role.NameRole != "Покупатель")
                return Forbid();

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

        // POST: api/Users/employees - Создание сотрудника (Админ)
        [HttpPost("employees")]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
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

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Email,
                user.FullName,
                Role = role.NameRole
            });
        }

        // PUT: api/Users/{id}/role - Изменение роли пользователя (Админ)
        [HttpPut("{id}/role")]
        [AuthorizeRole("Администратор")]
        public async Task<ActionResult> EditRole(int id, [FromBody] int roleId)
        {
            return await _userService.EditRole(id, roleId);
        }

        // PUT: api/Users/5 - Обновление пользователя (Админ, Менеджер)
        [HttpPut("{id}")]
        [AuthorizeRole("Администратор", "Менеджер")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var userRole = GetUserRole();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            if (userRole == "Менеджер" && user.Role.NameRole != "Покупатель")
                return Forbid();

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
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

            return NoContent();
        }

        // DELETE: api/Users/5 - Удаление пользователя (Админ)
        [HttpDelete("{id}")]
        [AuthorizeRole("Администратор")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CreateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Менеджер";
    }

    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AdressDelivery { get; set; }
        public string? Role { get; set; }
    }
}

