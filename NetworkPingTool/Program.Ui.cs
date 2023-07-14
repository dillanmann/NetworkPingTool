using MudBlazor.Services;
using NetworkPingTool.ViewModels;
using System.Reflection;

namespace NetworkPingTool
{
    public static class UiMiddlewareSetup
    {
        public static void AddUiServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddViewModels();
            builder.Services.AddMudServices();
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiRootUrl"]);
            });
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            var baseViewModelType = typeof(BaseViewModel);
            var viewModelTypes = Assembly.GetAssembly(baseViewModelType)
                .GetTypes()
                .Where(t => t.IsSubclassOf(baseViewModelType));

            foreach (var viewModelType in viewModelTypes)
            {
                services.AddScoped(viewModelType);
            }

            return services;
        }
    }
}
