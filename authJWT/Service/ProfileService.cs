using authJWT.Connection;
using authJWT.Interfaces;
using authJWT.Models;
using authJWT.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authJWT.Service
{
    public class ProfileService : IProfileService
    {
        private readonly ContextDb _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public ProfileService(ContextDb context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ActionResult> GetProfile(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new NotFoundObjectResult(new { message = "Пользователь не найден" });

            return new OkObjectResult(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Phone,
                user.AdressDelivery,
                user.CreatedAt,
                user.UpdatedAt,
                Role = user.Role.NameRole,
                PaymentMethods = user.PaymentMethods.Select(pm => new
                {
                    pm.Id,
                    pm.CardNumber,
                    pm.ExpiryDate
                })
            });
        }

        public async Task<ActionResult> UpdateProfile(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return new NotFoundObjectResult(new { message = "Пользователь не найден" });

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return new BadRequestObjectResult(new { message = "Пользователь с таким email уже существует" });
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;

            if (!string.IsNullOrEmpty(request.AdressDelivery))
                user.AdressDelivery = request.AdressDelivery;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        public async Task<ActionResult> AddPaymentMethod(int userId, AddPaymentMethodRequest request)
        {
            var paymentMethod = new PaymentMethod
            {
                CardNumber = request.CardNumber,
                ExpiryDate = request.ExpiryDate,
                CodeCVC = request.CodeCVC,
                UserId = userId
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetProfile), "Profile", new { }, paymentMethod);
        }

        public async Task<ActionResult> DeletePaymentMethod(int userId, int id)
        {
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId);

            if (paymentMethod == null)
                return new NotFoundObjectResult(new { message = "Платёжный метод не найден" });

            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}

