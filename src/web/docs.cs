using Microsoft.AspNetCore.Mvc;

namespace Nebula.web;

[ApiController]
[Route("api/[controller]")]

public class DocsController : ControllerBase
{
    private readonly dynamic _nebulaDocs = new
    {
        Init = new
        {
            command = "nebula init",
            description = "Initializes a nebula project.",
            usage = "nebula init"
        },
        Deploy = new
        {
            command = "nebula deploy",
            description = "Pushes code to the cloud.",
            usage = "nebula deploy"
        },
        Build = new
        {
            command = "nebula build",
            description = "Build docker image.",
            usage = "nebula build"
        }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_nebulaDocs);
    }

    [HttpGet("init")]
    public IActionResult GetInit()
    {
        return Ok(_nebulaDocs.Init);
    }

    [HttpGet("deploy")]
    public IActionResult GetDeploy()
    {
        return Ok(_nebulaDocs.Deploy);
    }

    [HttpGet("build")]
    public IActionResult GetBuild()
    {
        return Ok(_nebulaDocs.Build);
    }
}