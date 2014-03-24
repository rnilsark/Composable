﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Composable.CQRS.Windsor.Testing;
using Composable.ServiceBus;
using NUnit.Framework;

namespace AccountManagement.UI.QueryModels.Updaters.Tests.ContainerInstallers
{     
    public abstract class AccountManagementWiringTest
    {
        //Note that we do NOT test individual classes. We verify that when used as it will really be used things work as expected. 
        //If we change which installers exist, split installers, merge installers etc this test will keep working. 
        //We also make a base class that is abstract and inherit it twice in order to reuse the tests for both production and test wiring
        protected IWindsorContainer Container;

        [SetUp]
        public void WireContainer()
        {
            Container = new WindsorContainer();
            Container.ConfigureWiringForTestsCallBeforeAllOtherWiring();
            
            Container.Install(
                FromAssembly.Containing<QueryModels.Updaters.Services.IAccountManagementQueryModelUpdaterSession>()
            );

            Container.Register(
                Component.For<IServiceBus>().ImplementedBy<SynchronousBus>().LifestylePerWebRequest(),
                Component.For<IWindsorContainer>().Instance(Container)
                );
        }

        [Test]
        public void AllComponentsCanBeResolved()
        {
            Container
                .RegistrationAssertionHelper()
                .AllComponentsCanBeResolved();
        }

        [Test]
        public void EventStoreIsRegisteredScoped()
        {
            Container
                .RegistrationAssertionHelper()
                .LifestyleScoped<QueryModels.Updaters.Services.IAccountManagementQueryModelUpdaterSession>();
        }
    }

    [TestFixture]//The production wiring test does not modify the wiring at all
    public class AccountManagementProductionWiringTest : AccountManagementWiringTest
    {
    }

    [TestFixture]//The Test wiring test invokes all IConfigureWiringForTest instances in the container so that the wiring is now appropriate for running tests.
    public class AccountManagementTestWiringTest : AccountManagementWiringTest
    {
        [SetUp]
        public void SetupTask()
        {
            Container.ConfigureWiringForTestsCallAfterAllOtherWiring();
        }
    }
}