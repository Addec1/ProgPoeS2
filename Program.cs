using CMCS.Prototype.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC with views
builder.Services.AddControllersWithViews();

// Encryption key from config (fallback to a valid 32-byte key in hex)
var keyHex = builder.Configuration["Encryption:Key"]
    ?? "A1B2C3D4E5F60718293A4B5C6D7E8F90A1B2C3D4E5F60718293A4B5C6D7E8F90";

// Encrypted file store (App_Data/uploads + metadata.json)
builder.Services.AddSingleton<IFileStore>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var appData = Path.Combine(env.ContentRootPath, "App_Data");
    Directory.CreateDirectory(appData);
    return new EncryptedFileStore(appData, keyHex);
});

var app = builder.Build();

// Error handling: detailed in Dev, friendly in Prod
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Static files / routing
app.UseStaticFiles();
app.UseRouting();

// Attribute-routed controllers (e.g., /documents/download/{id})
app.MapControllers();

// Conventional MVC routes (Home/Index as default)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
