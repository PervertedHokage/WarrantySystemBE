using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WarrantySystem.API.Attributes;
using WarrantySystem.Repository.IRepositories;

namespace WarrantySystem.API.Middlewares;

public class DynamicAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public DynamicAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["ApiKey"] ?? throw new Exception("ApiKey missing in config");
    }

    public async Task InvokeAsync(HttpContext context, IUserPermissionService permissionService)
    {
        var endpoint = context.GetEndpoint();

        // 🔹 Check xem có gắn [ApiKeyAuthorize]
        var apiKeyAttr = endpoint?.Metadata.GetMetadata<ApiKeyAuthorizeAttribute>();
        if (apiKeyAttr != null)
        {
            bool isApiKey = context.Request.Headers.TryGetValue("x-api-key", out var apiKey);
            if (!isApiKey || apiKey != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing API Key");
                return;
            }

            context.Items["AuthType"] = "ApiKey";
            await _next(context);
            return;
        }

        // Check xem có gắn [RequiresPermission]
        var permissionAttributes = endpoint?.Metadata.GetOrderedMetadata<RequiresPermissionAttribute>();
        var authorizeAttribute = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>();

        //if (permissionAttributes != null && permissionAttributes.Count > 0)
        if (authorizeAttribute != null && authorizeAttribute.Count > 0)
        {
            bool? isAuthen = context.User.Identity?.IsAuthenticated;
            //Check có token không
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            //Check còn hạn không
            long expClaims = Convert.ToInt64(context.User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value);
            DateTime expires = DateTimeOffset.FromUnixTimeSeconds(expClaims).UtcDateTime; //.AddHours(+7);

            expires = new DateTime(expires.Year, expires.Month, expires.Day, expires.Hour, expires.Minute, 0);
            //DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            //if (now > expires)
            if (DateTime.UtcNow > expires)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Expired");
                return;
            }

            //var session = HttpContext.Session

            //Check là admin không
            var isAdminClaim = context.User.FindFirst("isadmin")?.Value;

            bool isAdmin = isAdminClaim switch
            {
                "1" => true,
                "0" => false,
                var v when bool.TryParse(v, out var parsed) => parsed,
                _ => false
            };

            if (isAdmin)
            {
                await _next(context);
                return;
            }

            //Check có mã quyền không
            if (permissionAttributes != null && permissionAttributes.Count > 0)
            {
                foreach (var attr in permissionAttributes)
                {
                    var hasPermission = await permissionService.HasPermissionAsync(userId, attr.Permission);
                    if (!hasPermission)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Access Denied");
                        return;
                    }
                }
            }

            await _next(context);
            return;
        }

        //Nếu không yêu cầu Authorize
        await _next(context);
    }
}