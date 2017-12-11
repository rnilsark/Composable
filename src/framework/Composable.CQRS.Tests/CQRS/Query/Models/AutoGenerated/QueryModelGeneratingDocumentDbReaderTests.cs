﻿using System;
using System.Collections.Generic;
using Composable.DependencyInjection;
using Composable.GenericAbstractions.Time;
using Composable.Messaging;
using Composable.Messaging.Events;
using Composable.Persistence.DocumentDb;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.AggregateRoots;
using Composable.Persistence.EventStore.Query.Models.Generators;
using Composable.Persistence.EventStore.Refactoring.Migrations;
using Composable.System.Linq;
using Composable.SystemExtensions.Threading;
using Composable.Tests.CQRS.Query.Models.AutoGenerated.Domain;
using Composable.Tests.CQRS.Query.Models.AutoGenerated.Domain.Events;
using Composable.Tests.CQRS.Query.Models.AutoGenerated.Domain.Events.Implementation;
using Composable.Tests.CQRS.Query.Models.AutoGenerated.Domain.Events.PropertyUpdated;
using Composable.Tests.CQRS.Query.Models.AutoGenerated.Domain.UI.QueryModels;
using FluentAssertions;
using NUnit.Framework;

namespace Composable.Tests.CQRS.Query.Models.AutoGenerated
{
    [TestFixture]
    public class QueryModelGeneratingDocumentDbReaderTests
    {
        IServiceLocator _serviceLocator;

        [SetUp]
        public void CreateContainer()
        {
            _serviceLocator = DependencyInjectionContainer.CreateServiceLocatorForTesting(
                                                                                    container => container.Register(

                                                                                                                    Component.For<IEventStore>()
                                                                                                                              .ImplementedBy<InMemoryEventStore>()
                                                                                                                              .LifestyleSingleton(),
                                                                                                                    Component.For<IEnumerable<IEventMigration>>()
                                                                                                                        .UsingFactoryMethod(kern => Seq.Empty<IEventMigration>())
                                                                                                                        .LifestyleSingleton(),
                                                                                                                    Component.For<IEventStoreUpdater, IEventStoreReader>()
                                                                                                                              .ImplementedBy<EventStoreUpdater>()
                                                                                                                              .LifestyleScoped(),
                                                                                                                    Component.For<IDocumentDbReader, IVersioningDocumentDbReader>()
                                                                                                                              .UsingFactoryMethod(kern => new QueryModelGeneratingDocumentDbReader(kern.Resolve<ISingleContextUseGuard>(), new []{kern.Resolve<AccountQueryModelGenerator>()}))
                                                                                                                              .LifestyleScoped(),
                                                                                                                    Component.For<AccountQueryModelGenerator> ()
                                                                                                                              .ImplementedBy<AccountQueryModelGenerator>()
                                                                                                                              .LifestyleScoped()
                                                                                                                   ));
        }

        [TearDown] public void TearDownTask()
        {
            _serviceLocator.Dispose();
        }

        [Test]
        public void ThrowsExceptionIfInstanceDoesNotExist()
        {
            using(_serviceLocator.BeginScope())
            {
                var reader = _serviceLocator.Resolve<IDocumentDbReader>();
                reader.Invoking(me => me.Get<MyAccountQueryModel>(Guid.NewGuid()))
                    .ShouldThrow<Exception>();
            }
        }

        [Test]
        public void CanFetchQueryModelAfterAggregateHasBeenCreated()
        {
            using(_serviceLocator.BeginScope())
            {
                var aggregates = _serviceLocator.Resolve<IEventStoreUpdater>();
                var accountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                var registered = _serviceLocator.ExecuteTransaction(() => MyAccount.Register(aggregates, accountId, "email", "password"));


                registered.Email.Should().Be("email");
                registered.Password.Should().Be("password");


                var reader = _serviceLocator.Resolve<IDocumentDbReader>();
                var loadedModel = reader.Get<MyAccountQueryModel>(registered.Id);

                loadedModel.Should().NotBe(null);
                loadedModel.Id.Should().Be(accountId);
                loadedModel.Email.Should().Be(registered.Email);
                loadedModel.Password.Should().Be(registered.Password);
            }
        }

        [Test]
        public void ThrowsExceptionWhenTryingToFetchDeletedEntity()
        {
            using (_serviceLocator.BeginScope())
            {
                var aggregates = _serviceLocator.Resolve<IEventStoreUpdater>();
                var accountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                var registered =_serviceLocator.ExecuteTransaction(() => MyAccount.Register(aggregates, accountId, "email", "password"));

                var reader = _serviceLocator.Resolve<IDocumentDbReader>();
                reader.Get<MyAccountQueryModel>(registered.Id);//Here it exists

                _serviceLocator.ExecuteTransaction(() => registered.Delete());

                using(_serviceLocator.BeginScope())
                {
                    var reader2 = _serviceLocator.Resolve<IDocumentDbReader>();
                    reader2.Invoking(me => me.Get<MyAccountQueryModel>(registered.Id))
                        .ShouldThrow<Exception>();
                }
            }
        }

        [Test]
        public void ReturnsUpdatedDataAfterTransactionHasCommitted()
        {
            using(_serviceLocator.BeginScope())
            {
                var accountId = Guid.Parse("00000000-0000-0000-0000-000000000001");


                var aggregates = _serviceLocator.Resolve<IEventStoreUpdater>();
                var registered = _serviceLocator.ExecuteTransaction(() => MyAccount.Register(aggregates, accountId, "email", "password"));


                _serviceLocator.Resolve<IDocumentDbReader>()
                    .Get<MyAccountQueryModel>(registered.Id); //Make sure we read it once so caches etc get involved.

                _serviceLocator.ExecuteTransaction(() => registered.ChangeEmail("newEmail"));

                using(_serviceLocator.BeginScope())
                {
                    var loadedModel = _serviceLocator.Resolve<IDocumentDbReader>()
                        .Get<MyAccountQueryModel>(registered.Id);

                    loadedModel.Should().NotBe(null);
                    loadedModel.Email.Should().Be("newEmail");
                }
            }
        }

        [Test]
        public void CanReturnPreviousVersionsOfQueryModel()
        {
            using (_serviceLocator.BeginScope())
            {
                var accountId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                var aggregates = _serviceLocator.Resolve<IEventStoreUpdater>();
                var registered = _serviceLocator.ExecuteTransaction(() => MyAccount.Register(aggregates, accountId, "originalEmail", "password"));

                _serviceLocator.Resolve<IVersioningDocumentDbReader>()
                    .GetVersion<MyAccountQueryModel>(registered.Id, registered.Version); //Make sure we read it once so caches etc get involved.

                _serviceLocator.ExecuteTransaction(() =>
                                                   {
                                                       registered.ChangeEmail("newEmail1");
                                                       registered.ChangeEmail("newEmail2");
                                                       registered.ChangeEmail("newEmail3");
                                                   });

                using (_serviceLocator.BeginScope())
                {
                    var loadedModel = _serviceLocator.Resolve<IVersioningDocumentDbReader>()
                        .Get<MyAccountQueryModel>(registered.Id);

                    loadedModel.Should().NotBe(null);
                    loadedModel.Email.Should().Be("newEmail3");

                    _serviceLocator.Resolve<IVersioningDocumentDbReader>()
                        .GetVersion<MyAccountQueryModel>(registered.Id, registered.Version -1)
                        .Email.Should().Be("newEmail2");

                    _serviceLocator.Resolve<IVersioningDocumentDbReader>()
                        .GetVersion<MyAccountQueryModel>(registered.Id, registered.Version - 2)
                        .Email.Should().Be("newEmail1");

                    _serviceLocator.Resolve<IVersioningDocumentDbReader>()
                        .GetVersion<MyAccountQueryModel>(registered.Id, registered.Version - 3)
                        .Email.Should().Be("originalEmail");
                }
            }
        }
    }

    namespace Domain
    {
        namespace UI
        {
            namespace QueryModels
            {
                public class MyAccountQueryModel : ISingleAggregateQueryModel
                {
                    public Guid Id { get; private set; }
                    internal string Email { get; set; }
                    internal string Password { get; set; }

                    public void SetId(Guid id)
                    {
                        Id = id;
                    }
                }

                public class AccountQueryModelGenerator : SingleAggregateQueryModelGenerator<AccountQueryModelGenerator, MyAccountQueryModel, IAccountEvent, IEventStoreReader>
                {
                    public AccountQueryModelGenerator(IEventStoreReader session) : base(session)
                    {
                        RegisterHandlers()
                            .For<IAccountEmailPropertyUpdatedEvent>(e => Model.Email = e.Email)
                            .For<IAccountPasswordPropertyUpdatedEvent>(e => Model.Password = e.Password);
                    }
                }
            }
        }


        class MyAccount : AggregateRoot<MyAccount,AccountEvent, IAccountEvent>
        {
            MyAccount():base(new DateTimeNowTimeSource())
            {
                RegisterEventAppliers()
                    .For<IAccountEmailPropertyUpdatedEvent>(e => Email = e.Email)
                    .For<IAccountPasswordPropertyUpdatedEvent>(e => Password = e.Password)
                    .For<IAccountDeletedEvent>(e => { });
            }

            public string Email { get; private set; }
            public string Password { get; private set; }

            public void ChangeEmail(string newEmail)
            {
                Publish(new EmailChangedEvent(newEmail));
            }

            public static MyAccount Register(IEventStoreUpdater aggregates, Guid accountId, string email, string password)
            {
                var registered = new MyAccount();
                registered.Publish(new AccountRegisteredEvent(accountId, email, password));
                aggregates.Save(registered);
                return registered;
            }

            public void Delete()
            {
                Publish(new AccountDeletedEvent());
            }
        }

        namespace Events
        {
            public interface IAccountEvent : IDomainEvent {}
            abstract class AccountEvent : DomainEvent, IAccountEvent
            {
                protected AccountEvent() { }
                protected AccountEvent(Guid aggregateRootId):base(aggregateRootId)
                {
                }

            }

            interface IAccountRegisteredEvent
                : IAggregateRootCreatedEvent,
                    IAccountEmailPropertyUpdatedEvent,
                    IAccountPasswordPropertyUpdatedEvent {}

            interface IEmailChangedEvent : IAccountEvent,
                IAccountEmailPropertyUpdatedEvent {}

            interface IAccountDeletedEvent : IAccountEvent,
                IAggregateRootDeletedEvent
            {

            }

            namespace PropertyUpdated
            {
                interface IAccountEmailPropertyUpdatedEvent : IAccountEvent
                {
                    string Email { get; }
                }

                interface IAccountPasswordPropertyUpdatedEvent : IAccountEvent
                {
                    string Password { get; }
                }
            }

            namespace Implementation
            {
                class AccountRegisteredEvent : AccountEvent, IAccountRegisteredEvent
                {
                    public AccountRegisteredEvent(Guid accountId, String email, string password) : base(accountId)
                    {
                        Email = email;
                        Password = password;
                    }

                    public string Email { get; private set; }
                    public string Password { get; private set; }
                }

                class EmailChangedEvent : AccountEvent, IEmailChangedEvent
                {
                    public EmailChangedEvent(string newEmail) => Email = newEmail;

                    public string Email { get; private set; }
                }

                class AccountDeletedEvent : AccountEvent, IAccountDeletedEvent
                {

                }
            }
        }
    }
}
