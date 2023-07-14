using NetworkPingTool;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices();
builder.AddUiServices();

var app = builder.Build();
app.AddApiMiddleware();

app.Run();