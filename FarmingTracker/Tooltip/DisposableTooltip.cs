using Blish_HUD.Controls;
using System.Reflection;

namespace FarmingTracker
{
    public class DisposableTooltip : Tooltip
    {
        protected override void DisposeControl()
        {
            RemoveFromStaticTooltips();
            base.DisposeControl();
        }

        // hack: workaround until memory leak in blish core is fixed https://github.com/blish-hud/Blish-HUD/issues/941
        private void RemoveFromStaticTooltips()
        {
            var allTooltipsField = typeof(Tooltip).GetField("_allTooltips", BindingFlags.NonPublic | BindingFlags.Static);

            if (allTooltipsField == null)
                return;

            var allTooltips = (ControlCollection<Tooltip>)allTooltipsField.GetValue(null);

            if (allTooltips == null)
                return;

            allTooltips.Remove(this);
        }
    }
}
