﻿using System;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ModuleSettingsView : View
    {
        public ModuleSettingsView(WindowTabSelector windowTabSelector)
        {
            _windowTabSelector = windowTabSelector;
        }

        protected override void Build(Container buildPanel)
        {
            _openSettingsButton = new OpenSettingsButton("Open Settings", _windowTabSelector, buildPanel);
            var x = Math.Max(buildPanel.Width / 2 - _openSettingsButton.Width / 2, 20);
            var y = Math.Max(buildPanel.Height / 2 - _openSettingsButton.Height / 2, 20);
            _openSettingsButton.Location = new Point(x, y);
        }

        protected override void Unload()
        {
            _openSettingsButton?.Dispose();
        }

        private OpenSettingsButton? _openSettingsButton;
        private readonly WindowTabSelector _windowTabSelector;
    }
}