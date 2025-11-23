using authJWT.Models;
using Microsoft.AspNetCore.Mvc;
using authJWT.Requests;
using Microsoft.AspNetCore.Identity;
using authJWT.Connection;
using authJWT.Service;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ContextDb _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(ContextDb context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromQuery] CreateCustomer request)
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

            if (string.IsNullOrWhiteSpace(request.Login_T))
                return BadRequest(new { message = "Логин не может быть пустым" });

            if (request.Login_T.Length < 3)
                return BadRequest(new { message = "Логин должен содержать минимум 3 символа" });

            if (string.IsNullOrWhiteSpace(request.AdressDelivery))
                return BadRequest(new { message = "Адрес доставки не может быть пустым" });

            if (request.CardNumber <= 0)
                return BadRequest(new { message = "Номер карты не может быть пустым" });

            if (string.IsNullOrWhiteSpace(request.ExpiryDate))
                return BadRequest(new { message = "Срок действия карты не может быть пустым" });

            if (request.CodeCVC <= 0 || request.CodeCVC > 999)
                return BadRequest(new { message = "CVC код должен быть от 1 до 999" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            if (await _context.Logins.AnyAsync(l => l.LoginT == request.Login_T))
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует" });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NameRole == "Покупатель");
            if (role == null)
            {
                return BadRequest(new { message = "Роль 'Покупатель' не найдена в системе" });
            }

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                AdressDelivery = request.AdressDelivery,
                RoleId = role.IdRole,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var login = new Login
            {
                LoginT = request.Login_T,
                Password = request.Password, 
                UserId = user.Id
            };

            _context.Logins.Add(login);

            var paymentMethod = new PaymentMethod
            {
                CardNumber = request.CardNumber,
                ExpiryDate = request.ExpiryDate,
                CodeCVC = request.CodeCVC,
                UserId = user.Id
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromQuery] string login, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                return BadRequest(new { message = "Логин не может быть пустым" });

            if (string.IsNullOrWhiteSpace(password))
                return BadRequest(new { message = "Пароль не может быть пустым" });

            var loginRecord = await _context.Logins
                .Include(l => l.Users)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(l => l.LoginT == login);

            if (loginRecord == null || loginRecord.Users == null)
            {
                return BadRequest(new { message = "Пользователь с таким логином не найден" });
            }

            var user = loginRecord.Users;
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest(new { message = "Неверный пароль" });
            }

            var token = _jwtService.GenerateToken(user, user.Role);

            var session = new Session
            {
                Token = token,
                UserId = user.Id
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    role = user.Role.NameRole
                }
            });
        }
    }
}
