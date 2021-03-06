﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Update data for viewing service
        public async Task UpdateDownloadFiles([TimerTrigger("00:01:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                await UpdateDownloadFilesAsync(log);
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", ex.Message);
                //Rethrow the error
                throw;
            }
        }

        //Update GPG download file
        public async Task UpdateDownloadFilesAsync(ILogger log, string userEmail = null, bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateDownloadFiles))) return;

            try
            {
                var returnYears = _SharedBusinessLogic.DataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Select(r => r.AccountingDate.Year)
                    .Distinct()
                    .ToList();

                //Get the downloads location
                var downloadsLocation = _SharedBusinessLogic.SharedOptions.DownloadsLocation;

                //Ensure we have a directory
                if (!await _SharedBusinessLogic.FileRepository.GetDirectoryExistsAsync(downloadsLocation))
                    await _SharedBusinessLogic.FileRepository.CreateDirectoryAsync(downloadsLocation);

                foreach (var year in returnYears)
                {
                    //If another server is already in process of creating a file then skip

                    var downloadFilePattern = $"GPGData_{year}-{year + 1}.csv";
                    var files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(downloadsLocation,
                        downloadFilePattern);
                    var oldDownloadFilePath = files.FirstOrDefault();

                    //Skip if the file already exists and is newer than 1 hour or older than 1 year
                    if (oldDownloadFilePath != null && !force)
                    {
                        var lastWriteTime =
                            await _SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(oldDownloadFilePath);
                        if (lastWriteTime.AddHours(1) >= VirtualDateTime.Now ||
                            lastWriteTime.AddYears(2) <= VirtualDateTime.Now) continue;
                    }

                    var returns = await _SharedBusinessLogic.DataRepository.GetAll<Return>().Where(r =>
                            r.AccountingDate.Year == year
                            && r.Status == ReturnStatuses.Submitted
                            && r.Organisation.Status == OrganisationStatuses.Active)
                        .ToListAsync();
                    returns.RemoveAll(r =>
                        r.Organisation.OrganisationName.StartsWithI(_SharedBusinessLogic.SharedOptions.TestPrefix));

                    var downloadData = returns.ToList()
                        .Select(r => DownloadResult.Create(r))
                        .OrderBy(d => d.EmployerName)
                        .ToList();

                    var newFilePath =
                        _SharedBusinessLogic.FileRepository.GetFullPath(Path.Combine(downloadsLocation,
                            $"GPGData_{year}-{year + 1}.csv"));
                    try
                    {
                        if (downloadData.Any())
                            await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, downloadData,
                                newFilePath, oldDownloadFilePath);
                        else if (!string.IsNullOrWhiteSpace(oldDownloadFilePath))
                            await _SharedBusinessLogic.FileRepository.DeleteFileAsync(oldDownloadFilePath);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
                }

                if (force && !string.IsNullOrWhiteSpace(userEmail))
                    try
                    {
                        await _Messenger.SendMessageAsync(
                            "UpdateDownloadFiles complete",
                            userEmail,
                            "The update of the download files completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateDownloadFiles));
            }
        }
    }
}