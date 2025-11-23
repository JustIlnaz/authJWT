using authJWT.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace authJWT.UniversalMethods
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAutorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int[] _rolesId;
        public RoleAutorizeAttribute(int[] roleId)
        {
            _rolesId = roleId;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ContextDb>();
            string? token = context.HttpContext.Request.Headers["Autorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(new { error = "Сессия не передана" }) { StatusCode = 401 };
                return;
            }

            var session = await dbContext.Sessions.Include(s => s.User).FirstOrDefaultAsync(s => s.Token == token);

            if (session == null)
            {
                context.Result = new JsonResult(new { error = "Сессия не найдена" }) { StatusCode = 401 };
                return;
            }

            if (!_rolesId.Contains(session.User.RoleId))
            {
                context.Result = new JsonResult(new { error = "Недостаточно прав" }) { StatusCode = 403 };
                return;
            }

            await next();
        }
    }

}