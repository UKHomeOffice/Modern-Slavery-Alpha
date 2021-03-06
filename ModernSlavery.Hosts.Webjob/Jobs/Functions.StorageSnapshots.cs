﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.Core.Extensions.Web;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task TakeSnapshotAsync([TimerTrigger(typeof(MidnightSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var azureStorageConnectionString = _StorageOptions.AzureConnectionString;
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true")) return;

                var connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                var azureStorageAccount = connectionString["AccountName"];
                var azureStorageKey = connectionString["AccountKey"];
                var azureStorageShareName = _StorageOptions.AzureShareName;

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);

                //Get the list of snapshots
                var response = await ListSnapshotsAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);
                var count = 0;
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var xml = XElement.Parse(response);
                    var snapshots =
                        xml.Descendants().Where(e => e.Name.LocalName.EqualsI("Snapshot")).Select(e => e.Value)
                            .ToList();
                    //var snapshots = snapshots.Where(e => e.EqualsI("Snapshot")).Select(e=>e.Value).ToList();
                    var deadline = VirtualDateTime.Now.AddDays(0 - _SharedBusinessLogic.SharedOptions.MaxSnapshotDays);
                    foreach (var snapshot in snapshots)
                    {
                        var date = DateTime.Parse(snapshot);
                        if (date > deadline) continue;

                        await DeleteSnapshotAsync(log, azureStorageAccount, azureStorageKey, azureStorageShareName,
                            snapshot);
                        count++;
                    }
                }

                log.LogDebug($"Executed {nameof(TakeSnapshotAsync)} successfully and deleted {count} stale snapshots");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob:{nameof(TakeSnapshotAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        public async Task TakeSnapshotAsync(ILogger log)
        {
            try
            {
                var azureStorageConnectionString = _StorageOptions.AzureConnectionString;
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true")) return;

                var connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                var azureStorageAccount = connectionString["AccountName"];
                var azureStorageKey = connectionString["AccountKey"];
                var azureStorageShareName = _StorageOptions.AzureShareName;

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);

                log.LogDebug($"Executed {nameof(TakeSnapshotAsync)} successfully");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed webjob:{nameof(TakeSnapshotAsync)}:{ex.Message}");
                throw;
            }
        }

        private async Task<string> TakeSnapshotAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "snapshot";
            var restype = "share";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "PUT\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/{shareName}\ncomp:{comp}\nrestype:{restype}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url = $"https://{storageAccount}.file.core.windows.net/{shareName}?restype={restype}&comp={comp}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var json = await CallApiAsync(HttpMethods.Put, url, headers: headers);
            return headers["x-ms-snapshot"];
        }


        private async Task<string> ListSnapshotsAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "list";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "GET\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/\ncomp:{comp}\ninclude:snapshots\nprefix:{shareName}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url =
                $"https://{storageAccount}.file.core.windows.net/?comp={comp}&prefix={shareName}&include=snapshots";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var response = await CallApiAsync(HttpMethods.Get, url, headers: headers);
            return response;
        }

        private async Task<string> DeleteSnapshotAsync(ILogger log,
            string storageAccount,
            string storageKey,
            string shareName,
            string snapshot)
        {
            var version = "2017-04-17";
            var restype = "share";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "DELETE\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/{shareName}\nrestype:{restype}\nsharesnapshot:{snapshot}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url =
                $"https://{storageAccount}.file.core.windows.net/{shareName}?sharesnapshot={snapshot}&restype={restype}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var response = await CallApiAsync(HttpMethods.Delete, url, headers: headers);

            log.LogDebug($"{nameof(DeleteSnapshotAsync)}: successfully deleted snapshot:{snapshot}");

            return headers["x-ms-request-id"];
        }


        private string SignAuthHeader(string canonicalizedString, string key, string account)
        {
            var unicodeKey = Convert.FromBase64String(key);
            using (var hmacSha256 = new HMACSHA256(unicodeKey))
            {
                var dataToHmac = Encoding.UTF8.GetBytes(canonicalizedString);
                return Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }
        }

        public async Task ArchiveAzureStorageAsync()
        {
            const string logZipDir = @"\Archive\";

            //Ensure the archive directory exists
            if (!await _SharedBusinessLogic.FileRepository.GetDirectoryExistsAsync(logZipDir))
                await _SharedBusinessLogic.FileRepository.CreateDirectoryAsync(logZipDir);

            //Create the zip file path using todays date
            var logZipFilePath = Path.Combine(logZipDir, $"{VirtualDateTime.Now.ToString("yyyyMMdd")}.zip");

            //Dont zip if we have one for today
            if (await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(logZipFilePath)) return;

            var zipDir = Url.UrlToDirSeparator(Path.Combine(_SharedBusinessLogic.FileRepository.RootDir, logZipDir));

            using (var fileStream = new MemoryStream())
            {
                var files = 0;
                using (var zipStream = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var dir in await _SharedBusinessLogic.FileRepository.GetDirectoriesAsync("\\", null, true))
                    {
                        if (Url.UrlToDirSeparator($"{dir}\\").StartsWithI(zipDir)) continue;

                        foreach (var file in await _SharedBusinessLogic.FileRepository.GetFilesAsync(dir, "*.*"))
                        {
                            var dirFile = Url.UrlToDirSeparator(file);

                            // prevents stdout_ logs
                            if (dirFile.ContainsI("stdout_")) continue;

                            var entry = zipStream.CreateEntry(dirFile);
                            using (var entryStream = entry.Open())
                            {
                                await _SharedBusinessLogic.FileRepository.ReadAsync(dirFile, entryStream);
                                files++;
                            }
                        }
                    }
                }

                if (files == 0) return;

                fileStream.Position = 0;
                await _SharedBusinessLogic.FileRepository.WriteAsync(logZipFilePath, fileStream);
            }
        }
    }
}