using System.Text;

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
            error = FormatException(ex),
            data = data
        };
    }

    private static string? FormatException(Exception ex)
    {
        if (ex == null) return null;
        var sb = new StringBuilder();

        int level = 0;
        Exception? current = ex;

        while (current != null)
        {
            sb.AppendLine($"--- Exception Level {level} ---");
            sb.AppendLine($"Type      : {current.GetType().FullName}");
            sb.AppendLine($"Message   : {current.Message}");
            sb.AppendLine($"Source    : {current.Source}");
            sb.AppendLine("StackTrace:");
            sb.AppendLine(current.StackTrace);
            sb.AppendLine();

            current = current.InnerException;
            level++;
        }

        return sb.ToString();
    }
}
