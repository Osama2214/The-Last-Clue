using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.Controllers;

public sealed class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();
}
