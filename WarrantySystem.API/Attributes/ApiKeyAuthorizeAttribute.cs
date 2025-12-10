using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WarrantySystem.API.Attributes;

public class ApiKeyAuthorizeAttribute : Attribute
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authType = context.HttpContext.Items["AuthType"]?.ToString();

        if (authType != "ApiKey")
        {
            context.Result = new UnauthorizedObjectResult(new { message = "API Key required ❌" });
        }
    }
}