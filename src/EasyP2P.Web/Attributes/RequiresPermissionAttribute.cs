using EasyP2P.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyP2P.Web.Attributes;

public class RequiresPermissionAttribute : ActionFilterAttribute
{
    private readonly string _permission;

    public RequiresPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userContextService = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();

        if (!userContextService.HasPermission(_permission))
        {
            context.Result = new ForbidResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}