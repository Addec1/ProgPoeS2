using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CloudIce3.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IHttpClientFactory _http;
    public HomeController(IHttpClientFactory http) => _http = http;

    public async Task<IActionResult> Index()
    {
        var me = User.Identity!.Name!;
        var c = _http.CreateClient("Functions");
        var resp = await c.GetAsync($"api/profile/{me}");
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (!resp.IsSuccessStatusCode)
            return View(new CloudIce3.Web.Models.ProfileGetResponse(me, "", me, ""));

        var profile = JsonSerializer.Deserialize<CloudIce3.Web.Models.ProfileGetResponse>(
            await resp.Content.ReadAsStringAsync(), opts);

        return View(profile);
    }
}
