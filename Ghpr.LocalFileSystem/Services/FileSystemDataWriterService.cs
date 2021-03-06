﻿using System;
using System.Collections.Generic;
using System.IO;
using Ghpr.Core.Common;
using Ghpr.Core.Extensions;
using Ghpr.Core.Interfaces;
using Ghpr.Core.Settings;
using Ghpr.Core.Utils;
using Ghpr.LocalFileSystem.Entities;
using Ghpr.LocalFileSystem.Extensions;
using Ghpr.LocalFileSystem.Interfaces;
using Ghpr.LocalFileSystem.Mappers;
using Ghpr.LocalFileSystem.Providers;
using Newtonsoft.Json;

namespace Ghpr.LocalFileSystem.Services
{
    public class FileSystemDataWriterService : IDataWriterService
    {
        private ILocationsProvider _locationsProvider;
        private ILogger _logger;

        public void InitializeDataWriter(ReporterSettings settings, ILogger logger)
        {
            _locationsProvider = new LocationsProvider(settings.OutputPath);
            _logger = logger;
        }
        
        public ItemInfoDto SaveRun(RunDto runDto)
        {
            var run = runDto.Map();
            var runGuid = run.RunInfo.Guid;
            var fileName = NamesProvider.GetRunFileName(runGuid);
            run.RunInfo.ItemName = fileName;
            _locationsProvider.RunsFolderPath.Create();
            var fullRunPath = _locationsProvider.GetRunFullPath(runGuid);
            using (var file = File.CreateText(fullRunPath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, run);
            }
            _logger.Info($"Run was saved: '{fullRunPath}'");
            var runsInfoFullPath = run.RunInfo.SaveRunInfo(_locationsProvider);
            _logger.Info($"Runs Info was saved: '{runsInfoFullPath}'");
            _logger.Debug($"Run data was saved correctly: {JsonConvert.SerializeObject(run, Formatting.Indented)}");
            return run.RunInfo.ToDto();
        }

        public SimpleItemInfoDto SaveScreenshot(TestScreenshotDto screenshotDto)
        {
            var testScreenshot = screenshotDto.Map();
            var path = _locationsProvider.GetScreenshotFolderPath(testScreenshot.TestGuid);
            testScreenshot.Save(path);
            _logger.Info($"Screenshot was saved: '{path}'");
            _logger.Debug($"Screenshot data was saved correctly: {JsonConvert.SerializeObject(testScreenshot, Formatting.Indented)}");
            return testScreenshot.TestScreenshotInfo.ToDto();
        }

        public void DeleteRun(ItemInfoDto runInfo)
        {
            var runFullPath = _locationsProvider.GetRunFullPath(runInfo.Guid);
            _logger.Debug($"Deleting Run: {runFullPath}");
            File.Delete(runFullPath);
            _locationsProvider.RunsFolderPath
                .DeleteItemsFromItemInfosFile(_locationsProvider.Paths.File.Runs, new List<ItemInfo>(1){runInfo.MapRunInfo()});
        }

        public void DeleteTest(TestRunDto testRun)
        {
            var testFullPath = _locationsProvider.GetTestFullPath(testRun.TestInfo.Guid, 
                testRun.TestInfo.Finish);
            _logger.Debug($"Deleting Test: {testFullPath}");
            File.Delete(testFullPath);
            _locationsProvider.GetTestFolderPath(testRun.TestInfo.Guid)
                .DeleteItemsFromItemInfosFile(_locationsProvider.Paths.File.Tests, new List<ItemInfo>(1) { testRun.TestInfo.MapTestRunInfo() });
        }

        public void DeleteTestOutput(TestRunDto testRun, TestOutputDto testOutput)
        {
            var testOutputFullPath = _locationsProvider.GetTestOutputFullPath(testRun.TestInfo.Guid, 
                testRun.TestInfo.Finish);
            _logger.Debug($"Deleting Test Output: {testOutputFullPath}");
            File.Delete(testOutputFullPath);
        }

        public void DeleteTestScreenshot(TestRunDto testRun, TestScreenshotDto testScreenshot)
        {
            var testScreenshotFullPath = _locationsProvider
                .GetTestScreenshotFullPath(testRun.TestInfo.Guid, testScreenshot.TestScreenshotInfo.Date);
            _logger.Debug($"Deleting Test screenshot: {testScreenshotFullPath}");
            File.Delete(testScreenshotFullPath);
        }

        public void SaveReportSettings(ReportSettingsDto reportSettingsDto)
        {
            var reportSettings = reportSettingsDto.Map();
            var fullPath = reportSettings.Save(_locationsProvider.SrcFolderPath, Paths.Files.ReportSettings);
            _logger.Info($"Report settings were saved: '{fullPath}'");
        }

        public ItemInfoDto SaveTestRun(TestRunDto testRunDto, TestOutputDto testOutputDto)
        {
            var testOutput = testOutputDto.Map();
            var testRun = testRunDto.Map(testOutput.TestOutputInfo.ToDto());
            var imgFolder = _locationsProvider.GetScreenshotFolderPath(testRun.TestInfo.Guid);
            if (Directory.Exists(imgFolder))
            {
                var imgFiles = new DirectoryInfo(imgFolder).GetFiles("*.json");
                _logger.Info($"Checking unassigned img files: {imgFiles.Length} file found");
                foreach (var imgFile in imgFiles)
                {
                    var img = Path.Combine(imgFolder, imgFile.Name).LoadTestScreenshot();
                    if (imgFile.CreationTime > testRun.TestInfo.Start)
                    {
                        _logger.Info($"New img file found: {imgFile.CreationTime}, {imgFile.Name}");
                        testRun.Screenshots.Add(img.TestScreenshotInfo);
                    }
                }
            }
            var testOutputFullPath = testOutput.Save(_locationsProvider.GetTestOutputFolderPath(testRun.TestInfo.Guid));
            _logger.Info($"Test output was saved: '{testOutputFullPath}'");
            _logger.Debug($"Test run data was saved correctly: {JsonConvert.SerializeObject(testOutput, Formatting.Indented)}");
            var testRunFullPath = testRun.Save(_locationsProvider.GetTestFolderPath(testRun.TestInfo.Guid));
            _logger.Info($"Test run was saved: '{testRunFullPath}'");
            var testRunsInfoFullPath = testRun.TestInfo.SaveTestInfo(_locationsProvider);
            _logger.Info($"Test runs Info was saved: '{testRunsInfoFullPath}'");
            _logger.Debug($"Test run data was saved correctly: {JsonConvert.SerializeObject(testRun, Formatting.Indented)}");
            return testRun.TestInfo.ToDto();
        }

        public void UpdateTestOutput(ItemInfoDto testInfo, TestOutputDto testOutput)
        {
            var outputFolderPath = _locationsProvider.GetTestOutputFolderPath(testInfo.Guid);
            var outputFileName = NamesProvider.GetTestOutputFileName(testInfo.Finish);
            var existingOutput = Path.Combine(outputFolderPath, outputFileName).LoadTestOutput();
            _logger.Debug($"Loaded existing output: {JsonConvert.SerializeObject(existingOutput, Formatting.Indented)}");
            existingOutput.SuiteOutput = testOutput.SuiteOutput;
            existingOutput.Output = testOutput.Output;
            File.Delete(Path.Combine(outputFolderPath, outputFileName));
            _logger.Debug("Deleted old output");
            existingOutput.Save(outputFolderPath);
            _logger.Debug($"Saved updated output: {JsonConvert.SerializeObject(existingOutput, Formatting.Indented)}");
        }
    }
}