using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CloudIce3.Web.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    public AuthController(IHttpClientFactory http, IConfiguration cfg) { _http = http; _cfg = cfg; }

    string JwtCookie => _cfg["Cookies:JwtCookieName"]!;

    [HttpGet] public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CloudIce3.Web.Models.RegisterRequest model)
    {
        var c = _http.CreateClient("Functions");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await c.PostAsync("api/auth/register", content);

        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await resp.Content.ReadAsStringAsync());
            return View(model);
        }

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var auth = JsonSerializer.Deserialize<CloudIce3.Web.Models.AuthResponse>(
            await resp.Content.ReadAsStringAsync(), opts)!;

        await SignInWithJwt(auth);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CloudIce3.Web.Models.LoginRequest model)
    {
        var c = _http.CreateClient("Functions");
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await c.PostAsync("api/auth/login", content);

        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid credentials.");
            return View(model);
        }

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var auth = JsonSerializer.Deserialize<CloudIce3.Web.Models.AuthResponse>(
            await resp.Content.ReadAsStringAsync(), opts)!;

        await SignInWithJwt(auth);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete(JwtCookie);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInWithJwt(CloudIce3.Web.Models.AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Username),
            new(ClaimTypes.Email, auth.Email)
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

        Response.Cookies.Append(JwtCookie, auth.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(6)
        });
    }
}
