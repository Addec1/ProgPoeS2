using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CloudIce3.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IHttpClientFactory _http;
    public ProfileController(IHttpClientFactory http) { _http = http; }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var me = User.Identity!.Name!;
        var c = _http.CreateClient("Functions");
        var resp = await c.GetAsync($"api/profile/{me}");
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var profile = JsonSerializer.Deserialize<CloudIce3.Web.Models.ProfileGetResponse>(
            await resp.Content.ReadAsStringAsync(), opts);

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string displayName, string email/*, IFormFile? profileImage*/)
    {
        var me = User.Identity!.Name!;
        string? blobUrl = null; // wire later

        var payload = new CloudIce3.Web.Models.ProfileUpdateRequest(email, displayName, blobUrl);
        var c = _http.CreateClient("Functions");
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await c.PutAsync($"api/profile/{me}", content);

        TempData[resp.IsSuccessStatusCode ? "Ok" : "Error"] =
            resp.IsSuccessStatusCode ? "Profile updated." : "Update failed.";

        return RedirectToAction("Index", "Home");
    }
}
