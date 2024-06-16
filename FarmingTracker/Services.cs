using Blish_HUD.Modules.Managers;
using System;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(
            Model model, 
            Gw2ApiManager gw2ApiManager, 
            SettingService settingService,
            Drf drf, 
            TextureService textureService, 
            FileSaveService fileSaveService, 
            FileLoadService fileLoadService)
        {
            Model = model;
            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            Drf = drf;
            TextureService = textureService;
            FileSaveService = fileSaveService;
            FileLoadService = fileLoadService;
        }

        public static async Task<Services> CreateServices(
            ContentsManager contentsManager, 
            DirectoriesManager directoriesManager, 
            Gw2ApiManager gw2ApiManager, 
            SettingService settingService)
        {
            var modelFilePath = FileService.GetModelFilePath(directoriesManager);
            var fileLoadService = new FileLoadService(modelFilePath);
            var model = await fileLoadService.LoadModelFromFile();
            var drf = new Drf(settingService);

            return new Services(
                model, 
                gw2ApiManager,
                settingService,
                drf,
                new TextureService(contentsManager),
                new FileSaveService(modelFilePath),
                fileLoadService);
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
        public Model Model { get; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
