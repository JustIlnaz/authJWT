using Microsoft.AspNetCore.Mvc;
using authJWT.Requests;

namespace authJWT.Interfaces
{
    public interface IProfileService
    {
        Task<ActionResult> GetProfile(int userId);
        Task<ActionResult> UpdateProfile(int userId, UpdateProfileRequest request);
        Task<ActionResult> AddPaymentMethod(int userId, AddPaymentMethodRequest request);
        Task<ActionResult> DeletePaymentMethod(int userId, int id);
    }
}

