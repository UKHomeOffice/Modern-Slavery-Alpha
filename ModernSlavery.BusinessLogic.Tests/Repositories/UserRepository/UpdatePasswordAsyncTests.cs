﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.BusinessLogic.Account.Abstractions;
using ModernSlavery.BusinessLogic.LogRecords;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.Tests.Common;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;
using Module = Autofac.Module;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdatePasswordAsyncTests : BaseTestFixture<UpdatePasswordAsyncTests.DependencyModule>
    {
        public class DependencyModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Initialise AutoMapper
                MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                    config.AddMaps(typeof(ModernSlavery.BusinessLogic.Account.Repositories.UserRepository));
                });
                builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            mockUserLogRecord = new Mock<IUserLogRecord>();

            // service under test
            testUserRepo =
                new ModernSlavery.BusinessLogic.Account.Repositories.UserRepository(mockDataRepo.Object, mockUserLogRecord.Object, DependencyContainer.Resolve<IMapper>());
        }

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IUserLogRecord> mockUserLogRecord;

        private IUserRepository testUserRepo;

        [TestCase]
        public async Task SavesNewPasswordHash()
        {
            // Arrange
            var saveChangesCalled = false;
            User testUserToUpdate = await testUserRepo.FindByEmailAsync("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com");
            var testPassword = "__Password123__";

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await testUserRepo.UpdatePasswordAsync(testUserToUpdate, testPassword);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            Assert.AreEqual(Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(testUserToUpdate.Salt)), testUserToUpdate.PasswordHash, "Expected to change password");
            Assert.AreEqual(HashingAlgorithm.PBKDF2, testUserToUpdate.HashingAlgorithm, "Expected hashing algorithm to change");
        }


        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Suspended)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            User testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            DateTime testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);
            string testExistingPasswordHash = Crypto.GetPBKDF2("ExistingPassword123", Convert.FromBase64String(testUserToUpdate.Salt));
            testUserToUpdate.PasswordHash = testExistingPasswordHash;
            testUserToUpdate.Status = testStatus;

            // Act
            var actualException = Assert.ThrowsAsync<ArgumentException>(
                async () => await testUserRepo.UpdatePasswordAsync(testUserToUpdate, "NewPassword123"));

            // Assert
            Assert.AreEqual(
                "Can only update passwords for active users. UserId=23322",
                actualException.Message,
                "Expected exception message to match");
            Assert.AreEqual(testExistingPasswordHash, testUserToUpdate.PasswordHash, "Expected password to still be the same");
            Assert.AreEqual(testStatus, testUserToUpdate.Status, "Expected status to still be the same");
        }

        private static object[] ThrowsErrorWhenArgumentIsNullCases = {
            new object[] {null, "newpassword123", "Value cannot be null.\r\nParameter name: userToUpdate"},
            new object[] {new User(), null, "Value cannot be null.\r\nParameter name: newPassword"},
            new object[] {new User(), "", "Value cannot be null.\r\nParameter name: newPassword"},
            new object[] {new User(), " ", "Value cannot be null.\r\nParameter name: newPassword"}
        };

        [TestCaseSource(nameof(ThrowsErrorWhenArgumentIsNullCases))]
        public void ThrowsErrorWhenArgumentIsNull(User testUserArg, string testPasswordArg, string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await testUserRepo.UpdatePasswordAsync(testUserArg, testPasswordArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}
