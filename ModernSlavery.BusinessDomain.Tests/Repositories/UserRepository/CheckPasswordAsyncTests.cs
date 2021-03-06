﻿using System;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Tests.Common;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Repositories.UserRepository
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class CheckPasswordAsyncTests : BaseTestFixture<CheckPasswordAsyncTests.DependencyModule>
    {
        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>();

            // service under test
            testUserRepo = new ModernSlavery.Infrastructure.Database.Classes.UserRepository(new DatabaseOptions(),
                new SharedOptions(), mockDataRepo.Object, Mock.Of<IUserLogger>(),
                DependencyContainer.Resolve<IMapper>());
        }

        public class DependencyModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Initialise AutoMapper
                var mapperConfig = new MapperConfiguration(config =>
                {
                    config.AddMaps(typeof(ModernSlavery.Infrastructure.Database.Classes.UserRepository));
                });
                builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            }
        }

        private Mock<IDataRepository> mockDataRepo;
        private IUserRepository testUserRepo;

        [Test]
        public async Task CorrectPasswordShouldResetLoginAttempts()
        {
            // Arrange
            var saveChangesCalled = false;
            var testPassword = "currentPassword123";
            var salt = "testSalt";
            var testUser = new User
            {
                PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(salt)),
                Salt = salt,
                HashingAlgorithm = HashingAlgorithm.PBKDF2, LoginAttempts = 3
            };

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            var actualResult = await testUserRepo.CheckPasswordAsync(testUser, testPassword);

            // Assert
            Assert.IsTrue(actualResult, "Expected correct password to return true");
            Assert.IsTrue(saveChangesCalled, "Expected save changes to be called");
            Assert.Zero(testUser.LoginAttempts, "Expected user login attempts to be 0");
        }

        [Test]
        public async Task IncorrectPasswordShouldIncreaseLoginAttempts()
        {
            // Arrange
            var testPassword = "currentPassword123";
            var salt = "testSalt";
            var testUser = new User
            {
                PasswordHash = Crypto.GetPBKDF2("WrongPassword123", Convert.FromBase64String(salt)), Salt = salt,
                HashingAlgorithm = HashingAlgorithm.PBKDF2, LoginAttempts = 0
            };
            var testAttempts = 3;

            for (var attempt = 1; attempt <= testAttempts; attempt++)
            {
                var saveChangesCalled = false;

                mockDataRepo.Setup(x => x.SaveChangesAsync())
                    .Callback(() => saveChangesCalled = true)
                    .Returns(Task.CompletedTask);

                // Act
                var actualResult = await testUserRepo.CheckPasswordAsync(testUser, testPassword);

                // Assert
                Assert.IsFalse(actualResult, "Expected wrong password to return false");
                Assert.IsTrue(saveChangesCalled, "Expected save changes to be called");
                Assert.AreEqual(attempt, testUser.LoginAttempts, $"Expected user login attempts to be {attempt}");
            }
        }
    }
}