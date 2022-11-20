using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application;

[ApiController]
[Route("home")]
public class TestController : Controller
{
    // GET
    public IActionResult Index()
    {
        throw new TestException("Test Extension");
    }
}