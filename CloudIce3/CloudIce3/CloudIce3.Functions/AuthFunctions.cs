using System.Linq;
using System.Net;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using CloudIce3.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CloudIce3.Functions;

public class AuthFunctions
{
    private readonly TableClient _users;
    private readonly JwtIssuer _jwt;
    private readonly JwtValidator _validator;

    public AuthFunctions(TableClient users, JwtIssuer jwt, JwtValidator validator)
    {
        _users = users; _jwt = jwt; _validator = validator;
    }

    [Function("Register")]
    public async Task<HttpResponseData> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await JsonSerializer.DeserializeAsync<RegisterRequest>(req.Body, opts);
        if (body is null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
            return await Bad(req, "Missing required fields");

        var username = body.Username.Trim().ToLowerInvariant();

        var byUser = await _users.GetEntityIfExistsAsync<UserEntity>("USER", username);
        if (byUser.HasValue) return await Conflict(req, "Username already taken");

        var byEmail = _users.Query<UserEntity>(e => e.PartitionKey == "USER" && e.Email == body.Email.Trim()).FirstOrDefault();
        if (byEmail is not null) return await Conflict(req, "Email already registered");

        var entity = new UserEntity
        {
            RowKey = username,
            Email = body.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(body.DisplayName) ? body.Username.Trim() : body.DisplayName!.Trim(),
            PasswordHash = PasswordHasher.Hash(body.Password),
            BlobUrl = "",
            Role = "User",
            IsActive = true
        };

        await _users.AddEntityAsync(entity);

        var token = _jwt.Issue(entity.RowKey, entity.Role, entity.Email);
        var resp = req.CreateResponse(HttpStatusCode.Created);
        await resp.WriteAsJsonAsync(new AuthResponse(token, entity.RowKey, entity.Email, entity.DisplayName, entity.BlobUrl));
        return resp;
    }

    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var body = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body, opts);
        if (body is null) return await Bad(req, "Invalid payload");

        var key = body.UsernameOrEmail.Trim().ToLowerInvariant();
        UserEntity? user = null;

        var byUser = await _users.GetEntityIfExistsAsync<UserEntity>("USER", key);
        if (byUser.HasValue) user = byUser.Value;
        else user = _users.Query<UserEntity>(e => e.PartitionKey == "USER" && e.Email == body.UsernameOrEmail.Trim()).FirstOrDefault();

        if (user is null || !user.IsActive || !PasswordHasher.Verify(body.Password, user.PasswordHash))
            return await Unauthorized(req, "Invalid credentials");

        var token = _jwt.Issue(user.RowKey, user.Role, user.Email);
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new AuthResponse(token, user.RowKey, user.Email, user.DisplayName, user.BlobUrl));
        return resp;
    }

    [Function("GetProfile")]
    public async Task<HttpResponseData> GetProfile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profile/{username}")] HttpRequestData req, string username)
    {
        var authHeader = GetAuthHeader(req);
        var principal = _validator.Validate(authHeader);
        if (principal is null || !string.Equals(principal.Identity?.Name, username, StringComparison.OrdinalIgnoreCase))
            return req.CreateResponse(HttpStatusCode.Forbidden);

        var ent = await _users.GetEntityIfExistsAsync<UserEntity>("USER", username.ToLowerInvariant());
        if (!ent.HasValue) return req.CreateResponse(HttpStatusCode.NotFound);

        var e = ent.Value;
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new ProfileGetResponse(e.RowKey, e.Email, e.DisplayName, e.BlobUrl));
        return resp;
    }

    [Function("UpdateProfile")]
    public async Task<HttpResponseData> UpdateProfile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "profile/{username}")] HttpRequestData req, string username)
    {
        var authHeader = GetAuthHeader(req);
        var principal = _validator.Validate(authHeader);
        if (principal is null || !string.Equals(principal.Identity?.Name, username, StringComparison.OrdinalIgnoreCase))
            return req.CreateResponse(HttpStatusCode.Forbidden);

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var payload = await JsonSerializer.DeserializeAsync<ProfileUpdateRequest>(req.Body, opts);
        if (payload is null) return await Bad(req, "Invalid payload");

        var ent = await _users.GetEntityIfExistsAsync<UserEntity>("USER", username.ToLowerInvariant());
        if (!ent.HasValue) return req.CreateResponse(HttpStatusCode.NotFound);

        var e = ent.Value;
        if (!string.IsNullOrWhiteSpace(payload.Email)) e.Email = payload.Email!.Trim();
        if (!string.IsNullOrWhiteSpace(payload.DisplayName)) e.DisplayName = payload.DisplayName!.Trim();
        if (!string.IsNullOrWhiteSpace(payload.BlobUrl)) e.BlobUrl = payload.BlobUrl!.Trim();

        await _users.UpdateEntityAsync(e, e.ETag, TableUpdateMode.Replace);

        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new ProfileGetResponse(e.RowKey, e.Email, e.DisplayName, e.BlobUrl));
        return resp;
    }

    private static string? GetAuthHeader(HttpRequestData req)
    {
        if (req.Headers.TryGetValues("Authorization", out var vals))
            return vals.FirstOrDefault();
        return null;
    }

    private static async Task<HttpResponseData> Bad(HttpRequestData req, string msg)
    { var r = req.CreateResponse(HttpStatusCode.BadRequest); await r.WriteStringAsync(msg); return r; }

    private static async Task<HttpResponseData> Unauthorized(HttpRequestData req, string msg)
    { var r = req.CreateResponse(HttpStatusCode.Unauthorized); await r.WriteStringAsync(msg); return r; }

    private static async Task<HttpResponseData> Conflict(HttpRequestData req, string msg)
    { var r = req.CreateResponse(HttpStatusCode.Conflict); await r.WriteStringAsync(msg); return r; }
}
