using Syncfusion.Maui.Toolkit.Charts;

namespace Díszkertész_kliens.Pages.Controls
{
    public class LegendExt : ChartLegend
    {
        protected override double GetMaximumSizeCoefficient()
        {
            return 0.5;
        }
    }
}
