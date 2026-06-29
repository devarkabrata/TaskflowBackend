using TaskFlowBackend.Data;

namespace TaskFlowBackend.Middleware;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDBContext dbContext)
    {
        if (HttpMethods.IsGet(context.Request.Method)     ||
            HttpMethods.IsHead(context.Request.Method)    ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await _next(context);

            if (context.Response.StatusCode < 400)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
