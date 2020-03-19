﻿using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Entities;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;

using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RemoveRetiredUserRegistrationsAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data
            DatabaseContext dbContext = AutoFacHelpers.CreateInMemoryTestDatabase(UserOrganisationHelper.CreateRegistrations());

            mockDataRepo = new SqlRepository(dbContext);
            mockLogRecordLogger = new Mock<IRegistrationLogRecord>();

            // service under test
            testRegistrationRepo =
                new ModernSlavery.Infrastructure.Data.RegistrationRepository(mockDataRepo, mockLogRecordLogger.Object);
        }

        private IDataRepository mockDataRepo;
        private Mock<IRegistrationLogRecord> mockLogRecordLogger;

        private IRegistrationRepository testRegistrationRepo;

        [Test]
        public async Task UnregistersAllOrganisationsForUser()
        {
            // Arrange
            User testRetiredUser = mockDataRepo.GetAll<User>()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testRetiredUser.UserOrganisations.FirstOrDefault();
            var calledLogUserAccountClosedAsync = 0;

            // Flag LogUserAccountClosedAsync
            mockLogRecordLogger.Setup(x => x.LogUserAccountClosedAsync(It.IsAny<UserOrganisation>(), It.IsAny<string>()))
                .Callback(
                    (UserOrganisation uo, string actionByEmail) => {
                        calledLogUserAccountClosedAsync++;
                        Assert.AreEqual(actionByEmail, testRetiredUser.EmailAddress, "Expected log action by email to match");
                    })
                .Returns(Task.CompletedTask);

            // Act
            await testRegistrationRepo.RemoveRetiredUserRegistrationsAsync(testRetiredUser, testRetiredUser);

            // Assert user org removed
            Assert.IsFalse(
                mockDataRepo.GetAll<UserOrganisation>().Any(uo => uo.UserId == testRetiredUser.UserId),
                "Expected no registrations");

            // Assert log
            Assert.AreEqual(2, calledLogUserAccountClosedAsync, "Expected LogUnregisteredSelfAsync to be called 2 times");
        }

    }

}
