using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers;

[ApiController]
//[Authorize]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class BaseController : ControllerBase
{
    
}