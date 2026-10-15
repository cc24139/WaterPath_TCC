using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace back_end.src.IA.Application;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IaExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not IaException error) return;

        context.HttpContext.RequestServices.GetService<ILogger<IaExceptionFilterAttribute>>()?
            .LogWarning("Falha segura na operação de IA. Codigo={ErrorCode} Status={StatusCode} TraceId={TraceId}",
                error.Code, error.StatusCode, context.HttpContext.TraceIdentifier);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = error.StatusCode,
            Title = "Não foi possível concluir a operação de IA.",
            Detail = error.Message,
            Extensions = { ["code"] = error.Code, ["traceId"] = context.HttpContext.TraceIdentifier },
        })
        { StatusCode = error.StatusCode };
        context.ExceptionHandled = true;
    }
}
