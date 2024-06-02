using Blish_HUD.Modules.Managers;
using System;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(ContentsManager contentsManager, DirectoriesManager directoriesManager, Gw2ApiManager gw2ApiManager, SettingService settingService)
        {
            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            Drf = new Drf(settingService);
            TextureService = new TextureService(contentsManager);
            var modelFilePath = FileService.GetModelFilePath(directoriesManager);
            FileLoadService = new FileLoadService(modelFilePath);
            FileSaveService = new FileSaveService(modelFilePath);
        }

        public void Dispose()
        {
            Drf?.Dispose();
            TextureService?.Dispose();
        }

        public UpdateLoop UpdateLoop { get; } = new UpdateLoop();
        public FontService FontService { get; } = new FontService();
        public TextureService TextureService { get; }
        public Gw2ApiManager Gw2ApiManager { get; }
        public SettingService SettingService { get; }
        public FileLoadService FileLoadService { get; }
        public FileSaveService FileSaveService { get; }
        public Drf Drf { get; }
        public Model Model { set;  get; } = new Model();
        public string SearchTerm { get; set; } = string.Empty;
    }
}
