﻿using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.LogEventLoggerProvider
{
    [TestFixture]
    public class ConstructorTests
    {
        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Infrastructure.Storage.AzureQueue>("TestConnectionString", "TestQueueName")
                {CallBase = true};
        }

        private Mock<Infrastructure.Storage.AzureQueue> mockQueue;

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenApplicationNameIsIllegal(string testAppName)
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() =>
                    new Infrastructure.Storage.LogEventLoggerProvider(mockQueue.Object, testAppName, null));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: applicationName", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenQueueIsNull()
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Infrastructure.Storage.LogEventLoggerProvider(null, "TestApplicationName",
                    new LoggerFilterOptions()));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queue", actualExpection.Message);
        }
    }
}