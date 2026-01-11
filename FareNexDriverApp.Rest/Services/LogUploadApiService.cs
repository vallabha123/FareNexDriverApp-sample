using FareNexDriverApp.Rest.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
﻿using System.IO.Compression;
using System.Net.Http.Headers;
using FareNexDriverApp.Core.Interfaces.Core;
using FareNexDriverApp.Core.Interfaces.Core.Configurations;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Rest;
using FareNexDriverApp.Core.Utilities;

namespace FareNexDriverApp.Rest.Services
{
    public class LogUploadApiService(HttpClient httpClient
            , ILogService<LogUploadApiService> logService
            , IAppConfigurationsService appConfigurationsService
            , ILocalizationService localizationService) : BaseApiService(httpClient, appConfigurationsService, localizationService), ILogUploadApiService
    {
        readonly ILogService<LogUploadApiService> _logService = logService;
        readonly string zipLogPath = Path.Combine(AppConstants.AppExternalDataFolderPath(), AppConstants.TempFolder, AppConstants.FareNexLogZipFileName);
        readonly DirectoryInfo zipDirectoryInfo = new(Path.Combine(AppConstants.AppExternalDataFolderPath(), AppConstants.TempFolder));

        /// <summary>
        /// Uploads log file to server as a ZIP
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        public async Task<bool> UploadLogsAsync(string? tripId)
        {
            bool isSuccess = false;
            try
            {
                _logService.LogMethodEntry();
                if (string.IsNullOrWhiteSpace(tripId))
                {
                    tripId = $"Random_{Guid.NewGuid()}";
                }
                string tempZipFilePath = string.Format(zipLogPath, tripId);
                if (await CreateZipFile(tempZipFilePath) && await ExecuteUploadPostApiCallAsync(tempZipFilePath))
                {
                    isSuccess = true;
                    PerformLogFolderCleanupPostUpload();
                    UploadFailedZipLogs();
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
            }
            _logService.LogMethodExit();
            return isSuccess;
        }

        /// <summary>
        /// Perform multi part form data Post api call to upload zip
        /// </summary>
        /// <param name="tempZipFilePath"></param>
        /// <returns></returns>
        async Task<bool> ExecuteUploadPostApiCallAsync(string tempZipFilePath)
        {
            bool success = false;
            string requestUri = GetRequestUri(ConfigurationKeys.LogUploadApi);
            _logService.LogInformation($"Rest call - {requestUri}");


            using FileStream fileStream = new(tempZipFilePath, FileMode.Open, FileAccess.Read);
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(AppConstants.HttpMultiPartZipMediaType);

            StringContent stringContent = new(_appConfigurationsService.LogBusNumber, System.Text.Encoding.UTF8, AppConstants.HttpMediaType);
            var formDataContent = new MultipartFormDataContent
            {
                { fileContent, "zipFile", Path.GetFileName(tempZipFilePath) },
                { stringContent, "busNumber" }
            };

            _httpClient!.DefaultRequestHeaders.Clear();
            _httpClient!.DefaultRequestHeaders.Add("LogAPIToken", _appConfigurationsService.GetAppConfigValue(ConfigurationKeys.LogUploadKey));
            HttpResponseMessage httpResponse = await _httpClient!.PostAsync(requestUri, formDataContent);

            _logService.LogInformation($"Rest call - Http response received");
            string responseContent = await httpResponse.Content.ReadAsStringAsync();
            _logService.LogInformation($"Rest call - Response Content - " + responseContent);
            if (httpResponse.IsSuccessStatusCode)
            {
                success = true;
                await fileStream.DisposeAsync();
                fileContent.Dispose();
                _logService.LogInformation("Zip file uploaded succesfully - " + tempZipFilePath);
                PerformZipFolderCleanupPostUpload(tempZipFilePath);
            }
            return success;
        }

        /// <summary>
        /// Create a ZIP under temp folder
        /// </summary>
        /// <param name="tempZipLogPath"></param>
        /// <returns></returns>
        async Task<bool> CreateZipFile(string tempZipLogPath)
        {
            bool isZipCreated = false;
            await Task.Run(() =>
            {
                try
                {
                    _logService.LogMethodEntry();
                    if (!zipDirectoryInfo.Exists)
                    {
                        zipDirectoryInfo.Create();
                    }
                    string logPath = Path.Combine(AppConstants.AppExternalDataFolderPath(), AppConstants.AppLogDataFolderName);
                    DirectoryInfo logDirectoryInfo = new(logPath);
                    if (logDirectoryInfo.Exists)
                    {
                        using ZipArchive zip = ZipFile.Open(tempZipLogPath, ZipArchiveMode.Create);
                        foreach (var file in logDirectoryInfo.EnumerateFiles())
                        {
                            zip.CreateEntryFromFile(file.FullName, file.Name, CompressionLevel.Optimal);
                        }
                        zip.Dispose();
                        isZipCreated = true;
                    }
                    _logService.LogInformation("Zip log file created - " + tempZipLogPath);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
                _logService.LogMethodExit();
            });
            return isZipCreated;
        }

        void PerformZipFolderCleanupPostUpload(string fullName)
        {
            if (zipDirectoryInfo.Exists)
            {
                File.Delete(fullName);
            }
        }

        void PerformLogFolderCleanupPostUpload()
        {
            DirectoryInfo logDirectoryInfo = new(Path.Combine(AppConstants.AppExternalDataFolderPath(), AppConstants.AppLogDataFolderName));
            if (logDirectoryInfo.Exists)
            {
                if (File.Exists(Path.Combine(logDirectoryInfo.FullName, AppConstants.GpsBreadCrumpsFileName)))
                {
                    File.Delete(Path.Combine(logDirectoryInfo.FullName, AppConstants.GpsBreadCrumpsFileName));
                }
                FileInfo[] fileInfos = logDirectoryInfo.GetFiles();
                if (fileInfos != null && fileInfos.Length > 0)
                {
                    foreach (FileInfo fileInfo in fileInfos.OrderBy(o => o.CreationTime).SkipLast(1))
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
                _logService.LogInformation("Uploaded previous log data");
                _logService.LogInformation(AppConstants.ApplicationNewLine);
                _logService.LogInformation("Application Version - " + AppInfo.Current.VersionString);
                _logService.LogInformation(AppConstants.ApplicationNewLine);
            }
        }

        /// <summary>
        /// Upload failed logs in background
        /// </summary>
        void UploadFailedZipLogs()
        {
            Task.Run(async () =>
            {
                _logService.LogMethodEntry("Uploading failed zip logs");
                try
                {
                    if (zipDirectoryInfo.Exists
                            && zipDirectoryInfo.GetFiles()?.Length > 0)
                    {
                        foreach (var fullName in zipDirectoryInfo.GetFiles().Select(s => s.FullName))
                        {
                            if (fullName.EndsWith(".zip") && !await ExecuteUploadPostApiCallAsync(fullName))
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex);
                }
                _logService.LogMethodExit();
            });
        }
    }
}
LogUploadApiService.cs
Displaying SseEventsApiService.cs.