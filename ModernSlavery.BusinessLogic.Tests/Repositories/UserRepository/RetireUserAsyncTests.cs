﻿using System;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Account.Abstractions;
using ModernSlavery.BusinessLogic.LogRecords;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RetireUserAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            mockUserLogRecord = new Mock<IUserLogRecord>();

            // service under test
            testUserRepo =
                new ModernSlavery.BusinessLogic.Account.Repositories.UserRepository(mockDataRepo.Object, mockUserLogRecord.Object);
        }

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IUserLogRecord> mockUserLogRecord;

        private IUserRepository testUserRepo;

        [TestCase]
        public async Task SavesRetiredStatus()
        {
            // Arrange
            var saveChangesCalled = false;
            User currentUser = await testUserRepo.FindByEmailAsync("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com");

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await testUserRepo.RetireUserAsync(currentUser);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            Assert.AreEqual(currentUser.Status, UserStatuses.Retired, "Expected to change status to retired");
            Assert.AreEqual(currentUser.StatusDetails, "User retired", "Expected retire status details to be set");
        }

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Suspended)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            DateTime testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);
            User currentUser = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            currentUser.Status = testStatus;

            // Act
            var actualException = Assert.ThrowsAsync<ArgumentException>(async () => await testUserRepo.RetireUserAsync(currentUser));

            // Assert
            Assert.AreEqual("Can only retire active users. UserId=23322", actualException.Message, "Expected exception message to match");
            Assert.AreEqual(testStatus, currentUser.Status, "Expected status to still be the same");
        }

        [TestCase(null, "Value cannot be null.\r\nParameter name: userToRetire")]
        public void ThrowsErrorWhenArgumentIsNull(User testUserArg, string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(async () => await testUserRepo.RetireUserAsync(testUserArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}
