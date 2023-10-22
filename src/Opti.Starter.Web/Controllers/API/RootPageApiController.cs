using Microsoft.AspNetCore.Mvc;

namespace Opti.Starter.Web.Controllers.API;

[ApiController]
public class RootPageApiController : Controller
{
    private readonly IContentLoader _contentLoader;

    public RootPageApiController(IContentLoader contentLoader)
    {
        _contentLoader = contentLoader;
    }

    [HttpGet("/api/v1/rootPage/")]
    public IActionResult Get()
    {
        try
        {
            _contentLoader.Get<IContent>(ContentReference.RootPage);
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }
}
