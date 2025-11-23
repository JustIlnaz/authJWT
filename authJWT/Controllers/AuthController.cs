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
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            if (await _context.Logins.AnyAsync(l => l.LoginT == request.Login_T))
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует" });
            }

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                AdressDelivery = request.AdressDelivery,
                RoleId = 3, // Покупатель
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
        public async Task<ActionResult> Login([FromBody] AuthorizeUser request)
        {
            var login = await _context.Logins
                .Include(l => l.Users)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(l => l.LoginT == request.Login);

            if (login == null || login.Users == null)
            {
                return BadRequest(new { message = "Пользователь с таким логином не найден" });
            }

            var user = login.Users;
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

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
