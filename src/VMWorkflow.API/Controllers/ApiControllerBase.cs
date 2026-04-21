using Microsoft.AspNetCore.Mvc;

namespace VMWorkflow.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string RequireAuthenticatedUsername() =>
        User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
}
