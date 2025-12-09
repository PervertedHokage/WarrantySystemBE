namespace WarrantySystem.Shared.Common;

public static class ApiResponseFactory
{
    public static APIResponse Success(object? data = null, string? message = "")
    {
        return new APIResponse
        {
            status = 1,
            message = message ?? "",
            data = data
        };
    }

    public static APIResponse Fail(Exception? ex, string message, object? data = null)
    {
        return new APIResponse
        {
            status = 0,
            message = message,
            error = ex?.ToString(),
            data = data
        };
    }

}
