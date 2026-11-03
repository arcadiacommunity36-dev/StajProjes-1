using System;
using System.Threading.Tasks;
using SimpleOrderSystem.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SimpleOrderSystem.Web.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorMiddleware> logger;

        public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BusinessException exception)
            {
                context.Response.StatusCode = exception.StatusCode;
                await context.Response.WriteAsJsonAsync(new { message = exception.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                context.Response.StatusCode = 409;
                await context.Response.WriteAsJsonAsync(new { message = "The record changed. Refresh the page and try again." });
            }
            catch (SqlException exception) when (exception.Number == 1205 || exception.Number == 3960)
            {
                context.Response.StatusCode = 409;
                await context.Response.WriteAsJsonAsync(new { message = "Another operation changed the same records. Refresh and try again." });
            }
            catch (DbUpdateException exception)
            {
                logger.LogError(exception, "A database operation failed.");
                context.Response.StatusCode = 409;
                await context.Response.WriteAsJsonAsync(new { message = "The record conflicts with existing data. Refresh and check related records." });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An unexpected operation failed.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { message = "The operation could not be completed." });
            }
        }
    }
}
