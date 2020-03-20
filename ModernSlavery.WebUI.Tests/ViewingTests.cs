﻿using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModernSlavery.WebUI.Controllers;
using ModernSlavery.WebUI.Models.Search;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.Tests
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ViewingTests
    {

        #region download

        [Test]
        [Description("Ensure the Download view form is returned for the current user")]
        public async Task Download_Get_SuccessAsync()
        {
            string originalDownloadsLocation =ConfigHelpers.GlobalOptions.DownloadsLocation; // remember so it can be reset at the end of this test

            // Arrange
           ConfigHelpers.GlobalOptions.DownloadsLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, "TestData");

            var firstFileTitle = "2006-2007";
            string firstFileLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, $"GPGData_{firstFileTitle}.csv");
            var secondFileTitle = "2005-2006";
            string secondFileLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, $"GPGData_{secondFileTitle}.csv");
            var thirdFileTitle = "2004-2005";
            string thirdFileLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, $"GPGData_{thirdFileTitle}.csv");

            ViewingController controller=null;
            try
            {
                var routeData = new RouteData();
                routeData.Values.Add("Action", "Download");
                routeData.Values.Add("Controller", "Viewing");
                controller = UiTestHelper.GetController<ViewingController>(0, routeData);

                await controller.CommonBusinessLogic.FileRepository.WriteAsync(firstFileLocation, Encoding.UTF8.GetBytes($"No data for {firstFileTitle}"));
                await controller.CommonBusinessLogic.FileRepository.WriteAsync(secondFileLocation, Encoding.UTF8.GetBytes($"No data for {secondFileTitle}"));
                await controller.CommonBusinessLogic.FileRepository.WriteAsync(thirdFileLocation, Encoding.UTF8.GetBytes($"No data for {thirdFileTitle}"));

                // Act
                var result = await controller.Download() as ViewResult;

                // Assert
                Assert.NotNull(result, "Expected ViewResult");
                Assert.AreEqual("Download", result.ViewName, "Incorrect view returned");

                var model = result.Model as DownloadViewModel;
                Assert.IsNotNull(model, "Expected DownloadViewModel or Incorrect resultType returned");
                Assert.IsNotNull(model.Downloads);
                Assert.True(result.ViewData.ModelState.IsValid, "Model was Invalid but was expected to be valid");
                Assert.AreEqual(3, model.Downloads.Count, $"Expected exactly 3 Downloads but were {model.Downloads.Count}");
                Assert.AreEqual(firstFileTitle, model.Downloads[0].Title);
                Assert.AreEqual(secondFileTitle, model.Downloads[1].Title);
                Assert.AreEqual(thirdFileTitle, model.Downloads[2].Title);
            }
            finally
            {
                // Cleanup
                if (controller != null)
                {
                    await controller.CommonBusinessLogic.FileRepository.DeleteFileAsync(firstFileLocation);
                    await controller.CommonBusinessLogic.FileRepository.DeleteFileAsync(secondFileLocation);
                    await controller.CommonBusinessLogic.FileRepository.DeleteFileAsync(thirdFileLocation);
                }

               ConfigHelpers.GlobalOptions.DownloadsLocation = originalDownloadsLocation;
            }
        }

        [Test]
        [Description("Ensure the Download Data view form is returned for the current user")]
        public async Task DownloadData_Get_SuccessAsync()
        {
            string originalDownloadsLocation =ConfigHelpers.GlobalOptions.DownloadsLocation; // remember so it can be reset at the end of this test

            // Arrange
           ConfigHelpers.GlobalOptions.DownloadsLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, "TestData");

            var firstFileTitle = "2001-2002";
            string firstFileLocation = Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, $"GPGData_{firstFileTitle}.csv");
            string firstFileContent = $"No content available for years {firstFileTitle}.";

            ViewingController controller = null;
            try
            {
                var routeData = new RouteData();
                routeData.Values.Add("Action", "DownloadData");
                routeData.Values.Add("Controller", "Viewing");

                controller = UiTestHelper.GetController<ViewingController>(0, routeData);
                await controller.CommonBusinessLogic.FileRepository.WriteAsync(firstFileLocation, Encoding.UTF8.GetBytes(firstFileContent));
                var year = 2001;

                // Act
                var result = await controller.DownloadData(year) as ContentResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(firstFileContent, result.Content, "Invalid download content returned");
            }
            finally
            {
                // Cleanup
                if (controller!=null)await controller.CommonBusinessLogic.FileRepository.DeleteFileAsync(firstFileLocation);
               ConfigHelpers.GlobalOptions.DownloadsLocation = originalDownloadsLocation;
            }
        }

        #endregion

    }
}
