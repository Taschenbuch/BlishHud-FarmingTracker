using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class CustomStatProfitRowPanel
    {
        public CustomStatProfitRowPanel(CustomStatProfit customStatProfit, Stat stat, HintLabel hintLabel, Model model, Services services, Container parent)
        {
            var iconSize = 50;
            var iconMargin = 1;
            var backgroundSize = iconSize + 2 * iconMargin;
            var backgroundMargin = 5;
            var panelHeight = backgroundSize + 2 * backgroundMargin;

            var statRowPanel = new Panel
            {
                BackgroundColor = Color.Black * 0.4f,
                Width = 450, // to have some room for long german/french names.
                Height = panelHeight,
                Parent = parent
            };

            // inventory slot background
            new Image(services.TextureService.InventorySlotBackgroundTexture)
            {
                Location = new Point(backgroundMargin),
                BasicTooltipText = stat.Details.Name,
                Size = new Point(backgroundSize),
                Parent = statRowPanel,
            };

            var statImage = new Image(services.TextureService.GetTextureFromAssetCacheOrFallback(stat.Details.IconAssetId))
            {
                Location = new Point(backgroundMargin + iconMargin),
                BasicTooltipText = stat.Details.Name,
                Size = new Point(iconSize),
                Parent = statRowPanel
            };

            var statNameLabel = new Label()
            {
                Location = new Point(statImage.Right + 10, backgroundMargin),
                Text = stat.Details.Name,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = statRowPanel
            };

            var coin = new Coin(customStatProfit.Unsigned_CustomProfitInCopper);

            var goldTextBox = new NumberTextBox(6) // 6 because otherwise end of number is hidden due to textbox not supporting horizontal scrolling.
            {
                Location = new Point(statImage.Right + 10, statNameLabel.Bottom + 5),
                Text = coin.Unsigned_Gold.ToString(),
                Width = 60,
                Parent = statRowPanel
            };

            var goldCoinImage = new Image(services.TextureService.SmallGoldCoinTexture)
            {
                Location = new Point(goldTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(goldTextBox.Height),
                Parent = statRowPanel,
            };

            var silverTextBox = new NumberTextBox(2)
            {
                Location = new Point(goldCoinImage.Right + 10, statNameLabel.Bottom + 5),
                Text = coin.Unsigned_Silver.ToString(),
                Width = 35,
                Parent = statRowPanel
            };

            var silverCoinImage = new Image(services.TextureService.SmallSilverCoinTexture)
            {
                Location = new Point(silverTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(silverTextBox.Height),
                Parent = statRowPanel,
            };

            var copperTextBox = new NumberTextBox(2)
            {
                Location = new Point(silverCoinImage.Right + 10, statNameLabel.Bottom + 5),
                Text = coin.Unsigned_Copper.ToString(),
                Width = 35,
                Parent = statRowPanel
            };

            goldTextBox.NumberTextChanged += (s, e) => OnCoinTextChanged(goldTextBox.Text, silverTextBox.Text, copperTextBox.Text, customStatProfit, services);
            silverTextBox.NumberTextChanged += (s, e) => OnCoinTextChanged(goldTextBox.Text, silverTextBox.Text, copperTextBox.Text, customStatProfit, services);
            copperTextBox.NumberTextChanged += (s, e) => OnCoinTextChanged(goldTextBox.Text, silverTextBox.Text, copperTextBox.Text, customStatProfit, services);

            // copper icon
            new Image(services.TextureService.SmallCopperCoinTexture)
            {
                Location = new Point(copperTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(copperTextBox.Height),
                Parent = statRowPanel,
            };

            var removeButton = new StandardButton
            {
                Text = "x",
                Width = 28,
                Parent = statRowPanel
            };

            removeButton.Location = new Point(statRowPanel.Width - removeButton.Width - 5, backgroundMargin);
            removeButton.Click += (s, e) =>
            {
                RemoveCustomStatProfit(customStatProfit, model, services);
                statRowPanel?.Dispose();
                CustomStatProfitTabView.ShowNoCustomStatProfitsExistHintIfNecessary(hintLabel, model);
            };
        }

        private static void OnCoinTextChanged(string goldText, string silverText, string copperText, CustomStatProfit customStatProfit, Services services)
        {
            goldText = string.IsNullOrWhiteSpace(goldText) ? "0" : goldText;
            silverText = string.IsNullOrWhiteSpace(silverText) ? "0" : silverText;
            copperText = string.IsNullOrWhiteSpace(copperText) ? "0" : copperText;

            int gold = int.Parse(goldText);
            int silver = int.Parse(silverText);
            int copper = int.Parse(copperText);

            customStatProfit.Unsigned_CustomProfitInCopper = 10000 * gold + 100 * silver + copper;

            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void RemoveCustomStatProfit(CustomStatProfit customStatProfit, Model model, Services services)
        {
            model.CustomStatProfits.RemoveSafe(customStatProfit);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }
    }
}
