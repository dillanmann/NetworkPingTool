using MudBlazor.Utilities;
using MudBlazor;

namespace NetworkPingTool.Shared
{
    public class Theme : MudTheme
    {
        // Colors
        private static readonly MudColor Background = new MudColor(255, 127, 80, 200);
        private static readonly MudColor Primary = new MudColor(47, 79, 79, 255);
        private static readonly MudColor Secondary = new MudColor(255, 255, 255, 255);

        public Theme()
        {
            Palette = new PaletteLight
            {
                Primary = Primary,
                Secondary = Secondary,
                Background = Background
            };
        }
    }
}
