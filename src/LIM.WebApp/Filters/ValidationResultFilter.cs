using LIM.ApplicationCore.Enums;
using LIM.WebApp.Extensions;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Filters;

public class ValidationActionResultFilter: IActionResult
{
    public async Task ExecuteResultAsync(ActionContext context)
    {
        #pragma warning disable
        var modelStateEntries = context.ModelState.Where(e => e.Value.Errors.Count > 0).ToArray();

        var errors = modelStateEntries
            .SelectMany(x => x.Value.Errors).ToList();

        var result = new AppResponce(errors.Select(x => new AppError(ErrorCode.ValidationError, x.ErrorMessage)));

        context.HttpContext.Response.StatusCode = StatusCodes.Status200OK; 

        await context.HttpContext.Response.WriteJsonAsync(result);
    }
}