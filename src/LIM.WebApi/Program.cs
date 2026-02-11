using System.Runtime.CompilerServices;
using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using LIM.Infrastructure;
using LIM.Infrastructure.ServiceConfiguration;
using LIM.WebApp.Filters;
using LIM.WebApp.ServiceConfigurations;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
LIM.Infrastructure.Serilog.LoggerHelper.AddLogger("LIM.WebApp");
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersioning();
builder.Services.AddSwaggerGen();
builder.Services.AddJwtAuthorization();

builder.Services.AddPostgresDbContext(builder.Configuration);
builder.Services.AddIdentityService(builder.Configuration);

//builder.Services.AddInMemoryDbContext();

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(HttpGlobalExceptionFilter));
});

builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<ApiBehaviorOptions>(op =>
{
    op.InvalidModelStateResponseFactory = _ => new ValidationActionResultFilter();
});
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container => container.RegisterModule(new ServiceModule()));

builder.Services.AddMvc().AddJsonOptions(option=>{
    option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    option.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    option.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //for dev
    app.Services.EnsureDbCreated();
}

//await app.Services.MigrateDbContextAsync();

app.UseRouting();
app.UseCors(options => options.AllowAnyOrigin());
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();