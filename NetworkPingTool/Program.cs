using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using NetworkPingTool.Hubs;
using NetworkPingTool.Services;
using NetworkPingTool.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IPingAddressService, PingAddressService>();
builder.Services.AddSingleton<IndexViewModel>();
builder.Services.AddMudServices();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          new[] { "application/octet-stream" });
});
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiRootUrl"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseResponseCompression();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapHub<PingResultHub>("/pingresulthub");
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
