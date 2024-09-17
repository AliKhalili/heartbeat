using System.Reflection;

namespace KestrelHeartbeat;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPut("/", context =>
            {
                if (context.Request.Headers.ContainsKey("Device-Id") && int.TryParse(context.Request.Headers["Device-Id"], out int deviceId))
                {
                    context.Response.StatusCode = 204;
                }
                else
                {
                    context.Response.StatusCode = 400;
                }

                return Task.CompletedTask;
            });
        });

        Console.WriteLine($"AspNetCore location: {typeof(IWebHostBuilder).GetTypeInfo().Assembly.Location}");
        Console.WriteLine($"AspNetCore version: {typeof(IWebHostBuilder).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

        Console.WriteLine($"NETCoreApp location: {typeof(object).GetTypeInfo().Assembly.Location}");
        Console.WriteLine($"NETCoreApp version: {typeof(object).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

    }
}