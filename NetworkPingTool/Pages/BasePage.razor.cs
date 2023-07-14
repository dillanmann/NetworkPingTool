using Microsoft.AspNetCore.Components;
using NetworkPingTool.ViewModels;

namespace NetworkPingTool.Pages
{
    public abstract class BasePage<TViewModel> : ComponentBase
        where TViewModel : BaseViewModel
    {
        [Inject]
        public TViewModel ViewModel { get; set; }

        protected override void OnInitialized() => ViewModel.OnInitialized();
        protected override async Task OnInitializedAsync() => await ViewModel.OnInitializedAsync();
        protected override void OnParametersSet() => ViewModel.OnParametersSet();
        protected override async Task OnParametersSetAsync() => await ViewModel.OnParametersSetAsync();

    }
}
