﻿using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class StatsPanels
    {
        public FlowPanel FarmedCurrenciesFlowPanel { get; internal set; }
        public FlowPanel FarmedItemsFlowPanel { get; internal set; }
        public ClickThroughImage CurrencyFilterIcon { get; internal set; }
        public ClickThroughImage ItemsFilterIcon { get; internal set; }
    }
}
