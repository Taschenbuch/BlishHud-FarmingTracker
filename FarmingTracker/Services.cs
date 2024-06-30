using Blish_HUD.Modules.Managers;
using System;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(
            ContentsManager contentsManager,
            DirectoriesManager directoriesManager,
            Gw2ApiManager gw2ApiManager, 
            SettingService settingService,
            DateTimeService dateTimeService)
        {
            var modelFilePath = FileService.GetModelFilePath(directoriesManager);

            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            DateTimeService = dateTimeService;
            TextureService = new TextureService(contentsManager);
            FileLoadService = new FileLoadService(modelFilePath);
            FileSaveService = new FileSaveService(modelFilePath);
            Drf = new Drf(settingService);
            FarmingDuration = new FarmingDuration(settingService);
        }

        public void Dispose()
        {
            Drf?.Dispose();
            TextureService?.Dispose();
            DateTimeService?.Dispose();
        }

        public Gw2ApiManager Gw2ApiManager { get; }
        public SettingService SettingService { get; }
        public TextureService TextureService { get; }
        public FileLoadService FileLoadService { get; }
        public FileSaveService FileSaveService { get; }
        public Drf Drf { get; }
        public FarmingDuration FarmingDuration { get; }
        public UpdateLoop UpdateLoop { get; } = new UpdateLoop();
        public FontService FontService { get; } = new FontService();
        public DateTimeService DateTimeService { get; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
