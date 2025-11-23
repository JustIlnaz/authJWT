using authJWT.Interfaces;
using authJWT.Requests;
using authJWT.Service;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace authJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Service.Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _service;

        public ProfileController(IProfileService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult> GetProfile()
        {
            return await _service.GetProfile(GetUserId());
        }

        [HttpPut]
        public async Task<ActionResult> UpdateProfile([FromQuery] UpdateProfileRequest request)
        {
            return await _service.UpdateProfile(GetUserId(), request);
        }

        [HttpPost("payment-methods")]
        [AuthorizeRole("Покупатель")]
        public async Task<ActionResult> AddPaymentMethod([FromQuery] AddPaymentMethodRequest request)
        {
            return await _service.AddPaymentMethod(GetUserId(), request);
        }

        [HttpDelete("payment-methods")]
        [AuthorizeRole("Покупатель")]
        public async Task<ActionResult> DeletePaymentMethod([FromQuery] int id)
        {
            return await _service.DeletePaymentMethod(GetUserId(), id);
        }
    }

    

   
}

