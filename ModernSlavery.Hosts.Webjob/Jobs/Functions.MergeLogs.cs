﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Merge all event log files from all instances into 1 single file per month
        public async Task MergeLogs([TimerTrigger("01:00:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Backup the log files first
                await ArchiveAzureStorageAsync();

                var actions = new List<Task>();

                #region WebServer Logs

                var webServerlogPath = Path.Combine(_SharedBusinessLogic.SharedOptions.LogPath, "ModernSlavery.WebUI");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "InfoLog"));
                actions.Add(MergeCsvLogsAsync<BadSicLogModel>(log, webServerlogPath, "BadSicLog"));
                actions.Add(MergeCsvLogsAsync<SubmissionLogModel>(log, webServerlogPath, "SubmissionLog"));
                actions.Add(MergeCsvLogsAsync<RegisterLogModel>(log, webServerlogPath, "RegistrationLog"));
                actions.Add(MergeCsvLogsAsync<SearchLogModel>(log, webServerlogPath, "SearchLog"));
                actions.Add(MergeCsvLogsAsync<UserLogModel>(log, webServerlogPath, "UserLog"));

                #endregion

                #region IdentityServer Logs

                var identityServerlogPath = Path.Combine(_SharedBusinessLogic.SharedOptions.LogPath,
                    "ModernSlavery.IdentityServer4");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "InfoLog"));

                #endregion

                #region Webjob Logs

                var webJoblogPath = Path.Combine(_SharedBusinessLogic.SharedOptions.LogPath, "ModernSlavery.WebJob");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "InfoLog"));
                actions.Add(MergeCsvLogsAsync<EmailSendLogModel>(log, webJoblogPath, "EmailSendLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "StannpSendLog"));
                actions.Add(MergeCsvLogsAsync<ManualChangeLogModel>(log, webJoblogPath, "ManualChangeLog"));

                #endregion

                await Task.WhenAll(actions);

                log.LogDebug($"Executed {nameof(MergeLogs)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(MergeLogs)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        private async Task MergeCsvLogsAsync<T>(ILogger log, string logPath, string prefix, string extension = ".csv")
        {
            //Get all the daily log files
            var files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(logPath, $"{prefix}_*{extension}");
            var fileList = files.OrderBy(o => o).ToList();

            //Get all files before today
            var startDate = VirtualDateTime.Now.Date;

            foreach (var file in fileList)
                try
                {
                    //Get the date from this daily log filename
                    var dateSuffix = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix) || dateSuffix.Length < 6) continue;

                    var date = dateSuffix.FromShortestDateTime();

                    //Ignore log files with no date in the filename
                    if (date == DateTime.MinValue) continue;

                    //Ignore todays daily log file 
                    if (date.Date >= startDate) continue;

                    //Get the monthly log file for this files date
                    var monthLog = Path.Combine(logPath, $"{prefix}_{date:yyMM}{extension}");

                    //Read all the records from this daily log file
                    var records = await _SharedBusinessLogic.FileRepository.ReadCSVAsync<T>(file);

                    //Add the records to its monthly log file
                    await _SharedBusinessLogic.FileRepository.AppendCsvRecordsAsync(monthLog, records);

                    //Delete this daily log file
                    await _SharedBusinessLogic.FileRepository.DeleteFileAsync(file);
                }
                catch (Exception ex)
                {
                    var message =
                        $"ERROR: Webjob ({nameof(MergeLogs)}) could not merge file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }

            var archiveDeadline = VirtualDateTime.Now.AddYears(-1).Date;

            foreach (var file in fileList)
                try
                {
                    //Get the date from this daily log filename
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var dateSuffix = fileName.AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix)) continue;

                    if (dateSuffix.Length != 4) continue;

                    //Get the date of this log from the filename
                    var year = dateSuffix.Substring(0, 2).ToInt32().ToFourDigitYear();
                    var month = dateSuffix.Substring(2, 2).ToInt32();
                    var logDate = new DateTime(year, month, 1);

                    //Dont archive logs newer than 1 year
                    if (logDate >= archiveDeadline) continue;

                    var archivePath = Path.Combine(logPath, year.ToString());
                    if (!await _SharedBusinessLogic.FileRepository.GetDirectoryExistsAsync(archivePath))
                        await _SharedBusinessLogic.FileRepository.CreateDirectoryAsync(archivePath);

                    //Ensure we have a unique filename
                    var ext = Path.GetExtension(file);
                    var archiveFilePath = Path.Combine(archivePath, fileName) + ext;

                    var c = 0;
                    while (await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(archiveFilePath))
                    {
                        c++;
                        archiveFilePath = Path.Combine(archivePath, fileName) + $" ({c}){ext}";
                    }

                    //Copy to the archive folder
                    await _SharedBusinessLogic.FileRepository.CopyFileAsync(file, archiveFilePath, false);

                    //Delete the old file
                    if (await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(archiveFilePath))
                        await _SharedBusinessLogic.FileRepository.DeleteFileAsync(file);
                }
                catch (Exception ex)
                {
                    var message =
                        $"ERROR: Webjob ({nameof(MergeLogs)}) could not archive file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }
        }
    }
}