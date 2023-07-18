namespace NetworkPingTool.ViewModels.Helpers
{
    public class Setting<T>
    {
        private T previousValue;
        public T Value { get; set; }

        public bool HasChanged { get => !Value.Equals(previousValue); }
        public void SaveChanges() => previousValue = Value;
    }
}
