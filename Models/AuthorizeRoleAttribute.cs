using automation.mbtdistr.ru.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace automation.mbtdistr.ru.Models
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
  public class AuthorizeRoleAttribute : Attribute, IAsyncActionFilter
  {
    private readonly RoleType[] _allowed;

    public AuthorizeRoleAttribute(params RoleType[] allowedRoles)
    {
      _allowed = allowedRoles;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      // Предполагаем, что где‑то ранее в middleware установлен HTTP‑заголовок "X-Telegram-Id"
      var telegramId = context.HttpContext.Request.Headers["X-Telegram-Id"].FirstOrDefault();
      if (string.IsNullOrEmpty(telegramId))
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      // Достаём из БД роль
      var db = context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext))
               as ApplicationDbContext;
      var worker = await db.Workers
          .FindAsync(int.Parse(telegramId)); // или по TelegramId, если хранится строкой

      if (worker == null || !_allowed.Contains(worker.Role))
      {
        context.Result = new ForbidResult();
        return;
      }

      await next();
    }
  }

}
