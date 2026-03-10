using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmingTracker 
{
    // Custom because I use CustomContextMenuStripItems, which are not displayed when added to ContextMenuStrip
    public class CustomContextMenuStrip : Container
    {
        private const int BORDER_PADDING = 2;
        private const int ITEM_WIDTH = 135;
        private const int ITEM_HEIGHT = 22;
        private const int ITEM_VERTICALMARGIN = 6;
        private const int CONTROL_WIDTH = BORDER_PADDING + ITEM_WIDTH + BORDER_PADDING;
        private const int CLICK_DEBOUNCE = 100;
        private static readonly List<WeakReference<CustomContextMenuStrip>> _contextMenuStrips = new List<WeakReference<CustomContextMenuStrip>>();
        private static readonly Texture2D _textureMenuEdge = Content.GetTexture("scrollbar-track");
        private static double _lastOpenTime;
        private (Point Position, int DownOffset, int UpOffset) _targetOffset;

        static CustomContextMenuStrip() 
        {
            Input.Mouse.LeftMouseButtonPressed += HandleMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += HandleMouseButtonPressed;
        }

        private static void RegisterContextMenuStrip(CustomContextMenuStrip contextMenuStrip) 
        {
            lock (_contextMenuStrips)
            {
                _contextMenuStrips.Add(new WeakReference<CustomContextMenuStrip>(contextMenuStrip));
            }
        }

        private static void HandleMouseButtonPressed(object sender, MouseEventArgs e) 
        {
            // Debounce to prevent mistakenly closing the menu immediatley after opening (or when
            // this event is triggered after the same event that triggered it to open)
            if (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastOpenTime < CLICK_DEBOUNCE) return;

            lock (_contextMenuStrips)
            {
                WeakReference<CustomContextMenuStrip>[] allMenuStrips = _contextMenuStrips.ToArray();

                if ( Input.Mouse.ActiveControl is CustomContextMenuStrip)
                    return;

                foreach (var cmsRef in allMenuStrips)
                {
                    if (!cmsRef.TryGetTarget(out var cms))
                    {
                        _contextMenuStrips.Remove(cmsRef);
                        continue;
                    }

                    if (!cms.Visible) 
                        continue;

                    if (!cms.MouseOver) 
                        cms.Hide();
                }
            }
        }


        public CustomContextMenuStrip()
        {
            Visible = false;
            Width = CONTROL_WIDTH;
            ZIndex = Screen.CONTEXTMENU_BASEINDEX;
            RegisterContextMenuStrip(this);
        }

        protected override void OnShown(EventArgs e)
        {
            // Keep track of when we opened for debounce
            _lastOpenTime = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
            Parent = GameService.Graphics.SpriteScreen;

            // If we have no children, don't display (and don't even call 'Shown' event)
            if (_children.IsEmpty)
            {
                Visible = false;
                return;
            }

            base.OnShown(e);
        }

        protected override void OnHidden(EventArgs e)
        {
            Parent = null;
            base.OnHidden(e);
        }

        private int GetVerticalOffset(int yStart, int downOffset = 0, int upOffset = 0)
        {
            int yUnderDef = Graphics.SpriteScreen.Bottom - (yStart + _size.Y);
            int yAboveDef = Graphics.SpriteScreen.Top + (yStart - _size.Y);

            return yUnderDef > 0 || yUnderDef > yAboveDef
                ? yStart + upOffset // flip down
                : yStart - _size.Y + downOffset; // flip up
        }

        private void SetPositionFromOffset((Point Position, int DownOffset, int UpOffset) offset)
        {
            Location = new Point(offset.Position.X, GetVerticalOffset(offset.Position.Y, offset.DownOffset, offset.UpOffset));
        }

        public void Show(Point position)
        {
            SetPositionFromOffset(_targetOffset = (position, 0, 0));

            base.Show();
        }

        public void Show(Control activeControl)
        {
            if (activeControl is CustomContextMenuStripItem parentMenu)
            {
                SetPositionFromOffset(_targetOffset = (new Point(parentMenu.AbsoluteBounds.Right - 3, parentMenu.AbsoluteBounds.Top), 19, 0));

                ZIndex = parentMenu.Parent.ZIndex + 1;
            } else
            {
                SetPositionFromOffset(_targetOffset = (activeControl.AbsoluteBounds.Location, 0, activeControl.Height));
            }

            base.Show();
        }

        public override void Hide() 
        {
            Visible = false;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e) 
        {
            base.OnChildAdded(e);
            OnChildMembershipChanged(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);
            OnChildMembershipChanged(e);
        }

        private void OnChildMembershipChanged(ChildChangedEventArgs e)
        {
            if (e.Added)
            {
                if (!(e.ChangedChild is CustomContextMenuStripItem newChild))
                {
                    e.Cancel = true;
                    return;
                }

                newChild.Height = ITEM_HEIGHT;

                newChild.Resized += ChildOnResized;
            } else
            {
                e.ChangedChild.Resized -= ChildOnResized;
            }

            if (Visible)
            {
                SetPositionFromOffset(_targetOffset);
            }

            Invalidate();
        }

        private void ChildOnResized(object sender, ResizedEventArgs e) 
        {
            Invalidate();
        }

        public override void RecalculateLayout()
        {
            if (!_children.IsEmpty)
            {
                int maxChildWidth = CONTROL_WIDTH;

                int lastChildBottom = BORDER_PADDING - ITEM_VERTICALMARGIN;

                foreach (var menuItem in _children.Where(c => c.Visible))
                {
                    maxChildWidth = Math.Max(menuItem.Width, maxChildWidth);

                    menuItem.Location = new Point(BORDER_PADDING, lastChildBottom + ITEM_VERTICALMARGIN);

                    lastChildBottom = menuItem.Bottom;
                }

                _size = new Point(maxChildWidth + BORDER_PADDING * 2,
                                  lastChildBottom + BORDER_PADDING);

                foreach (var childItem in Children)
                {
                    childItem.Width = maxChildWidth;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(BORDER_PADDING,
                                                 BORDER_PADDING,
                                                 _size.X - BORDER_PADDING * 2,
                                                 _size.Y - BORDER_PADDING * 2),
                                   Color.FromNonPremultiplied(33, 32, 33, 255));

            // Left line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - BORDER_PADDING),
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - BORDER_PADDING),
                                   Color.White * 0.8f);

            // Top line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   Color.White * 0.8f,
                                   -MathHelper.PiOver2,
                                   Vector2.Zero);

            // Bottom line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(1, _size.Y, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   new Rectangle(1, BORDER_PADDING, _textureMenuEdge.Width, _size.X - BORDER_PADDING),
                                   Color.White * 0.8f,
                                   -MathHelper.PiOver2,
                                   Vector2.Zero);

            // Right line
            spriteBatch.DrawOnCtrl(this,
                                   _textureMenuEdge,
                                   new Rectangle(_size.X - _textureMenuEdge.Width, 1, _textureMenuEdge.Width, _size.Y - 2),
                                   new Rectangle(0, 1, _textureMenuEdge.Width, _size.Y - 2),
                                   Color.White * 0.8f);
        }
    }
}
