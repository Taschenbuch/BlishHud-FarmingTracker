using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Linq;

namespace FarmingTracker
{
    public class DebugTabView : View
    {
        public DebugTabView(Model model, Services services)
        {
            _model = model;
            _services = services;
        }

        protected override void Unload()
        {
            _rootFlowPanel?.Dispose();
            _rootFlowPanel = null;
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            _rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(20, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            new HintLabel(
                _rootFlowPanel,
                "If you can see this tab, you are running a debug instead of a release version of this module.\n" +
                "Do not change any settings here. They will not speed up or improve anything.\n" +
                "They will rather break the module.\n" +
                "This tab just helps the developer to test the module. :-)");

            CreateDrfDebugPanel(_rootFlowPanel);
            _services.DateTimeService.CreateDateTimeDebugPanel(_rootFlowPanel);
        }

        private void CreateDrfDebugPanel(Container parent)
        {
            var debugDrfFlowPanel = new FlowPanel
            {
                Title = "DRF Debug",
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(5, 5),
                ShowBorder = true,
                BackgroundColor = Color.Black * 0.3f,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            new SettingControl(debugDrfFlowPanel, _services.SettingService.IsFakeDrfServerUsedSetting);

            var dropToClipboardButton = new StandardButton
            {
                Text = "Copy stats to clipboard as DRF drop",
                Width = 300,
                Parent = debugDrfFlowPanel
            };

            dropToClipboardButton.Click += async (s, e) =>
            {
                var drfMessage = ConvertToDrfMessage(_model);
                var drfMessageAsString = JsonConvert.SerializeObject(drfMessage);
                await ClipboardUtil.WindowsClipboardService.SetTextAsync($"'{drfMessageAsString}',"); // format for fake drf server message list
            };
        }

        private static DrfMessage ConvertToDrfMessage(Model model)
        {
            var snapshot = model.Stats.StatsSnapshot;
            var items = snapshot.ItemById.Values.ToList();
            var currencies = snapshot.CurrencyById.Values.ToList();

            var drfMessage = new DrfMessage();
            drfMessage.Kind = "data";
            drfMessage.Payload.Character = "1";

            foreach (var item in items.Where(s => s.Count != 0))
                drfMessage.Payload.Drop.Items.Add(item.ApiId, item.Count);

            foreach (var currency in currencies.Where(s => s.Count != 0).Take(DrfWebSocketClient.MAX_CURRENCIES_IN_A_SINGLE_DROP))
                drfMessage.Payload.Drop.Currencies.Add(currency.ApiId, currency.Count);

            return drfMessage;
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
    }
}
