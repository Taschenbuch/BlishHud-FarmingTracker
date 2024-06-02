using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class FileSaveService
    {
        public FileSaveService(string modelFilePath)
        {
            _modelFilePath = modelFilePath;
        }

        public async Task SaveModelToFile(Model model)
        {
            try
            {
                var fileModel = FileModelService.CreateFileModel(model);
                var fileModelJson = JsonConvert.SerializeObject(fileModel);
                await WriteFileAsync(fileModelJson, _modelFilePath);
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Error: Failed to saving model to file. :(");
            }
        }

        private static async Task WriteFileAsync(string fileContent, string filePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(folderPath);
            using var streamWriter = new StreamWriter(filePath);
            await streamWriter.WriteAsync(fileContent);
            await streamWriter.FlushAsync();
        }

        private readonly string _modelFilePath;
    }
}