﻿using AccountManagement.TestHelpers.Fixtures;
using Composable.Contracts;
using NUnit.Framework;

namespace AccountManagement.Domain.Tests.AccountTests
{
    public class ChangeEmailFailureScenariosTests : DomainTestBase
    {
        private Account _account;

        [SetUp]
        public void RegisterAccount()
        {
            _account = new AccountRegisteredFixture().Setup(Container);
        }

        [Test]
        public void WhenEmailIsNullObjectIsNullExceptionIsThrown()
        {
            Assert.Throws<ObjectIsNullException>(() => _account.ChangeEmail(null));
        }
    }
}