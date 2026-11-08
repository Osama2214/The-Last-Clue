using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase;
