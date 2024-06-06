using Blish_HUD.Modules.Managers;
using System.IO;

namespace FarmingTracker
{
    public class FileService
    {
        public static string GetModelFilePath(DirectoriesManager directoriesManager)
        {
            var moduleFolderName = directoriesManager.RegisteredDirectories[0];
            var moduleFolderPath = directoriesManager.GetFullDirectoryPath(moduleFolderName);
            return Path.Combine(moduleFolderPath, "model.json");
        }
    }
}