using Azure.Data.Tables;
using CloudIce3.Functions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
  .ConfigureAppConfiguration((ctx, cfg) =>
  {
      cfg.AddJsonFile("appsettings.json", optional: true)
         .AddJsonFile("local.settings.json", optional: true)
         .AddEnvironmentVariables();
  })
  .ConfigureFunctionsWorkerDefaults()
  .ConfigureServices((ctx, services) =>
  {
      var cs = ctx.Configuration["StorageConnection"]!;
      services.AddSingleton(new TableClient(cs, "Users"));
      services.AddSingleton<JwtIssuer>();
      services.AddSingleton<JwtValidator>();
  })
  .Build();

host.Run();
