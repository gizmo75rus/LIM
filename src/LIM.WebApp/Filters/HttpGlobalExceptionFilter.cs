using System.Collections.ObjectModel;
using System.Net;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LIM.WebApp.Filters;

public class HttpGlobalExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HttpGlobalExceptionFilter> _logger;

    public HttpGlobalExceptionFilter(IWebHostEnvironment environment, ILogger<HttpGlobalExceptionFilter> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var errors = new Collection<AppError>();

        if (context.Exception is CommonException exp)
        {
            errors.Add(new AppError(exp.ErrorCode,exp.Message));
            context.Result = new OkObjectResult(new AppResponce(errors));
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            context.ExceptionHandled = true;


            return;
        }

        _logger.LogError(new EventId(context.Exception.HResult),
            context.Exception,
            context.Exception.Message);


        if (_environment.IsDevelopment())
        {
            errors.Add(new AppError(ErrorCode.UnhandeledExeption, context.Exception.Message));
        }
        else
        {
            errors.Add(new AppError(ErrorCode.UnhandeledExeption,"Произошла непредвиденная ошибка. Повторите попытку позже"));
        }

        context.Result = new OkObjectResult(new AppResponce(errors));
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        context.ExceptionHandled = true;
    }
}