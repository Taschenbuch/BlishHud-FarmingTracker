using Blish_HUD.Modules.Managers;
using System.IO;

namespace FarmingTracker
{
    public class FileService
    {
        public static string GetModelFilePath(string moduleFolderPath)
        {
            return Path.Combine(moduleFolderPath, "model.json");
        }

        public static string GetModuleFolderPath(DirectoriesManager directoriesManager)
        {
            var moduleFolderName = directoriesManager.RegisteredDirectories[0];
            return directoriesManager.GetFullDirectoryPath(moduleFolderName);
        }
    }
}