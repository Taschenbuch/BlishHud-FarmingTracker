using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class StatsPanels
    {
        public StatsPanels(
            FlowPanel currenciesFlowPanel, 
            FlowPanel favoriteItemsFlowPanel, 
            FlowPanel itemsFlowPanel, 
            ClickThroughImage currencyFilterIcon, 
            ClickThroughImage itemsFilterIcon)
        {
            CurrenciesFlowPanel = currenciesFlowPanel;
            FavoriteItemsFlowPanel = favoriteItemsFlowPanel;
            ItemsFlowPanel = itemsFlowPanel;
            CurrencyFilterIcon = currencyFilterIcon;
            ItemsFilterIcon = itemsFilterIcon;
        }

        public FlowPanel CurrenciesFlowPanel { get; }
        public FlowPanel ItemsFlowPanel { get; }
        public FlowPanel FavoriteItemsFlowPanel { get; }
        public ClickThroughImage CurrencyFilterIcon { get; }
        public ClickThroughImage ItemsFilterIcon { get; }
    }
}
