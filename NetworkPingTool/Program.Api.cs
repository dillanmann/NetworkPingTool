using Microsoft.AspNetCore.ResponseCompression;
using NetworkPingTool.Hubs;
using NetworkPingTool.Services;

namespace NetworkPingTool
{
    public static class ApiMiddlewareSetup
    {
        public static void AddApiMiddleware(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseResponseCompression();
            }


            app.UseStaticFiles();

            app.UseRouting();
            app.MapBlazorHub();
            app.MapHub<PingResultHub>("/pingresulthub");
            app.MapFallbackToPage("/_Host");
            app.MapControllers();
        }

        public static void AddApiServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<IPingAddressService, PingAddressService>();
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                      new[] { "application/octet-stream" });
            });
        }
    }
}
