using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//TODO: do tests for role
public class AuthorizeUserAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public AuthorizeUserAttribute(string role = null)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var username = context.HttpContext.Session.GetString("Username");
        var role = context.HttpContext.Session.GetString("Role");

        if (string.IsNullOrEmpty(username))
        {
            context.Result = new RedirectToActionResult("Login", "Users", null);
            return;
        }

        if (!string.IsNullOrEmpty(_role) && role != _role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Users", null); 
        }
    }
}

