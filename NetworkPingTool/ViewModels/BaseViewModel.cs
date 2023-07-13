namespace NetworkPingTool.ViewModels
{
    public abstract class BaseViewModel
    {
        public virtual void OnParametersSet() { }
        public virtual void OnInitialized() { }
        public virtual async Task OnInitializedAsync() => await Task.CompletedTask;
        public virtual async Task OnParametersSetAsync() => await Task.CompletedTask;
    }
}
