using System.Diagnostics;
using System.Security.Claims;
using LastClue.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using LastClue.Web.Models;

namespace LastClue.Web.Controllers;

public sealed class HomeController(IUserRepository users) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = id is null ? null : await users.GetUserAsync(id, cancellationToken);
        return View(user is null ? null : new AvatarViewModel(user.DisplayName, user.AvatarSeed));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
