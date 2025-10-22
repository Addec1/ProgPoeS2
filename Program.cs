using CMCS.Prototype.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


var keyHex = builder.Configuration["Encryption:Key"]
    ?? "A1B2C3D4E5F60718293A4B5C6D7E8F90A1B2C3D4E5F60718293A4B5C6D7E8F90";

builder.Services.AddSingleton<IFileStore>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var appData = Path.Combine(env.ContentRootPath, "App_Data");
    Directory.CreateDirectory(appData);
    return new EncryptedFileStore(appData, keyHex);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
