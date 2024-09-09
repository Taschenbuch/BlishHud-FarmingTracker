using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class FileSaver
    {
        public FileSaver(string modelFilePath)
        {
            _modelFilePath = modelFilePath;
        }

        // because in module.unload() it should not be async
        public void SaveModelToFileSync(Model model)
        {
            try
            {
                var fileModelJson = SerializeModelToJson(model);
                File.WriteAllText(_modelFilePath, fileModelJson);
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Error: Failed to save model to file. :(");
            }
        }

        public async Task SaveModelToFile(Model model)
        {
            try
            {
                var fileModelJson = SerializeModelToJson(model);
                await WriteFileAsync(_modelFilePath, fileModelJson);
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Error: Failed to saving model to file. :(");
            }
        }
        public static async Task WriteFileAsync(string filePath, string fileContent)
        {
            var folderPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(folderPath);
            using var streamWriter = new StreamWriter(filePath);
            await streamWriter.WriteAsync(fileContent);
            await streamWriter.FlushAsync();
        }

        private static string SerializeModelToJson(Model model)
        {
            var fileModel = FileModelService.CreateFileModel(model);
            return JsonConvert.SerializeObject(fileModel);
        }

        private readonly string _modelFilePath;
    }
}