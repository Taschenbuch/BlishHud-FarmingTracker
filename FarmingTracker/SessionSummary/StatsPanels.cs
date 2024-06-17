using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class StatsPanels
    {
        public FlowPanel CurrenciesFlowPanel { get; internal set; }
        public FlowPanel ItemsFlowPanel { get; internal set; }
        public ClickThroughImage CurrencyFilterIcon { get; internal set; }
        public ClickThroughImage ItemsFilterIcon { get; internal set; }
    }
}
