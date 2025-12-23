using InstitutoAdecco.Data;
using InstitutoAdecco.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstitutoAdecco.Areas.Admin;

[Area("Admin")]
[Authorize]
public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {       
        return View();
    }
}
