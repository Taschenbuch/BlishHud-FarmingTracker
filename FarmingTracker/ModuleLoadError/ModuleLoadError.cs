using Blish_HUD.Graphics.UI;
using System;

namespace FarmingTracker
{
    public class ModuleLoadError : IDisposable
    {
        public bool HasModuleLoadFailed { get; set; }

        public void InitializeErrorSettingsViewAndShowErrorWindow(string errorWindowTitle, string errorText)
        {
            HasModuleLoadFailed = true;
            _errorSettingsView = new ErrorSettingsView(errorText);
            _errorWindow = new ErrorWindow(errorWindowTitle, errorText);
            _errorWindow.Show();
        }

        public IView CreateErrorSettingsView()
        {
            return _errorSettingsView;
        }

        public void Dispose()
        {
            _errorWindow?.Dispose();
        }

        private ErrorSettingsView _errorSettingsView;
        private ErrorWindow _errorWindow;
    }
}
