﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class CsvFileExporter
    {
        public CsvFileExporter(string moduleFolderPath)
        {
            ModuleFolderPath = moduleFolderPath;
        }

        public string ModuleFolderPath { get; }

        public async Task ExportSummaryAsCsvFile(Model model)
        {
            try
            {
                var csvFileText = CreateCsvFileText(model);
                var csvFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss_fff}.csv";
                var csvFolderPath = Path.Combine(ModuleFolderPath, "csv");
                var csvFilePath = Path.Combine(csvFolderPath, csvFileName);
                await FileSaver.WriteFileAsync(csvFilePath, csvFileText);
                Process.Start("explorer.exe", csvFolderPath);
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, "Failed to save csv file. :(");
            }
        }

        private static string CreateCsvFileText(Model model)
        {
            var snapShot = model.Stats.StatsSnapshot;
            var items = snapShot.ItemById.Values.Where(s => s.Signed_Count != 0).ToList();
            var currencies = snapShot.CurrencyById.Values.Where(s => s.Signed_Count != 0).ToList();
            var linesCount = Math.Max(items.Count, currencies.Count);

            var csvFileText = "item_id,item_name,item_amount,currency_id,currency_amount\n";

            for (int i = 0; i < linesCount; i++)
            {
                csvFileText += i < items.Count
                    ? $"{items[i].ApiId},{EscapeCsvField(items[i].Details.Name)},{items[i].Signed_Count},"
                    : ",,,";

                csvFileText += i < currencies.Count
                    ? $"{currencies[i].ApiId},{currencies[i].Signed_Count}\n"
                    : ",\n";
            }

            return csvFileText;
        }

        // add quote before every quote.
        // then wrap everything in quotes. This handles comma (,) inside the field, too.
        // e.g.:
        // BEFORE: Story Unlock: "The Dragon's Reach, Part 2"
        // AFTER: "Story Unlock: ""The Dragon's Reach, Part 2"""
        // Excel seem to have no issues with special characters like slashes and stuff.
        private static string EscapeCsvField(string field)
        {
            var csvEscapedField = field.Replace("\"", "\"\""); 
            return $"\"{csvEscapedField}\""; 
        }
    }
}
