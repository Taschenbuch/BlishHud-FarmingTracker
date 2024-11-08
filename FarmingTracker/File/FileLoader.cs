using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class FileLoader
    {
        public FileLoader(string modelFilePath)
        {
            _modelFilePath = modelFilePath;
        }

        public async Task<Model> LoadModelFromFile()
        {
            var isFirstModuleStart = !File.Exists(_modelFilePath);
            return isFirstModuleStart
                ? new Model()
                : await LoadModelFromModuleFolder(_modelFilePath);
        }

        private static async Task<Model> LoadModelFromModuleFolder(string modelFilePath)
        {
            try
            {
                var fileModelJson = await GetFileContentAndThrowIfFileEmpty(modelFilePath);
                var fileModel = JsonConvert.DeserializeObject<FileModel>(fileModelJson);

                if (fileModel == null)
                    throw new InvalidOperationException("Deserializing the model.json file failed.");

                return ModelCreator.CreateModel(fileModel);
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Error: Failed to load local model from file. fallback: use empty Stats :(");
                return new Model();
            }
        }

        private static async Task<string> GetFileContentAndThrowIfFileEmpty(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var streamReader = new StreamReader(fileStream);
            var fileContent = await streamReader.ReadToEndAsync();

            // Because JsonConvert.DeserializeObject returns null for empty string. no idea why file is empty sometimes (was reported in sentry)
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new InvalidOperationException("file is empty!");

            return fileContent;
        }

        private readonly string _modelFilePath;
    }
}