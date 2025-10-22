using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.AccessDeniedPath = "/Auth/Login";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.ExpireTimeSpan = TimeSpan.FromHours(6);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CloudIce3.Web.AuthTokenHandler>();
builder.Services.AddHttpClient("Functions", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    c.BaseAddress = new Uri(cfg["FunctionsBaseUrl"]!);
}).AddHttpMessageHandler<CloudIce3.Web.AuthTokenHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.Run();
