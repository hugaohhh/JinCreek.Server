using JinCreek.Server.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace JinCreek.Server.Admin.Services
{
    // see https://docs.microsoft.com/ja-jp/aspnet/core/web-api/handle-errors?view=aspnetcore-3.1#use-exceptions-to-modify-the-response
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var problemDetailsFactory = context.HttpContext?.RequestServices?.GetRequiredService<ProblemDetailsFactory>();

            if (context.Exception is EntityNotFoundException exception)
            {
                var d = new ModelStateDictionary();
                d.AddModelError(exception.Message, Messages.NotFound);
                context.Result =
                    new ObjectResult(problemDetailsFactory?.CreateValidationProblemDetails(context.HttpContext, d, statusCode: StatusCodes.Status404NotFound))
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                context.ExceptionHandled = true;
            }
        }
    }
}
