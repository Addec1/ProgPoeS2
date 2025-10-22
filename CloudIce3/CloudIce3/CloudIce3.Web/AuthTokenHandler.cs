using System.Net.Http.Headers;

namespace CloudIce3.Web;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;
    private readonly IConfiguration _cfg;

    public AuthTokenHandler(IHttpContextAccessor ctx, IConfiguration cfg)
    {
        _ctx = ctx; _cfg = cfg;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var cookieName = _cfg["Cookies:JwtCookieName"];
        var token = _ctx.HttpContext?.Request.Cookies[cookieName!];
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(req, ct);
    }
}