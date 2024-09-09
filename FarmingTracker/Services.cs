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
            var moduleFolderPath = FileService.GetModuleFolderPath(directoriesManager);
            var modelFilePath = FileService.GetModelFilePath(moduleFolderPath);

            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            DateTimeService = dateTimeService;
            TextureService = new TextureService(contentsManager);
            CsvFileExporter = new CsvFileExporter(moduleFolderPath);
            FileLoader = new FileLoader(modelFilePath);
            FileSaver = new FileSaver(modelFilePath);
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
        public CsvFileExporter CsvFileExporter { get; }
        public FileLoader FileLoader { get; }
        public FileSaver FileSaver { get; }
        public Drf Drf { get; }
        public FarmingDuration FarmingDuration { get; }
        public UpdateLoop UpdateLoop { get; } = new UpdateLoop();
        public FontService FontService { get; } = new FontService();
        public DateTimeService DateTimeService { get; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
