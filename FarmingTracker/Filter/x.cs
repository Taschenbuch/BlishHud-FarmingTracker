using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace FarmingTracker
{
    public class CustomPanel : FlowPanel
    {
        public Scrollbar Scrollbar => this.Parent?.Children.OfType<Scrollbar>().FirstOrDefault(s => s.AssociatedContainer == this);

        private float _targetScrollDistance = 0f;

        public float ScrollDistance
        {
            get => Scrollbar?.ScrollDistance ?? 0f;
            set
            {
                if (!CanScroll || Scrollbar == null)
                    return;

                Scrollbar.ScrollDistance = MathHelper.Clamp(value, 0f, 1f);
            }
        }

        public void SaveScrollDistance()
        {
            if (Scrollbar != null && !float.IsNaN(Scrollbar.ScrollDistance))
            {
                _targetScrollDistance = Scrollbar.ScrollDistance * (float)(this.Height - Scrollbar.Size.Y);
            }
        }

        public void UpdateScrollDistance(float target)
        {
            if (Scrollbar != null && !float.IsNaN(target))
            {
                float distance = (float)target / (float)(this.Height - Scrollbar.Size.Y);

                Scrollbar.ScrollDistance = MathHelper.Clamp(distance, 0f, 1f);
            }
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            e.ChangedChild.Resized += ChangedChild_Resized;
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);

            e.ChangedChild.Resized -= ChangedChild_Resized;
        }

        private void ChangedChild_Resized(object sender, ResizedEventArgs e)
        {
            if (CanScroll)
                SaveScrollDistance();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (_targetScrollDistance > 0f)
            {
                UpdateScrollDistance(_targetScrollDistance);
                _targetScrollDistance = 0f;
            }
        }

        public void ScrollToChild(Control child)
        {
            if (!CanScroll || !Children.Contains(child) || Scrollbar == null)
                return;

            if (child.Location.Y == 0)
                Scrollbar.ScrollDistance = 0f;
            else
                Scrollbar.ScrollDistance = (float)child.Location.Y / (float)(Children.Where(c => c.Visible).Max(c => c.Bottom) - Scrollbar.Size.Y);
        }

        protected override void DisposeControl()
        {
            foreach (var child in Children)
                child.Resized -= ChangedChild_Resized;

            base.DisposeControl();
        }
    }
}
