﻿using System;
using AccountManagement.Domain.Events;
using Composable.DependencyInjection;
using Composable.Functional;
using Composable.Messaging.Buses;
using Composable.Persistence.DocumentDb;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;

namespace AccountManagement.UI.QueryModels
{
    static class AccountStatistics
    {
        /// <summary>
        /// Note that we use a <see cref="SelfGeneratingQueryModel{TQueryModel,TAggregateEvent}"/> even though we will store it in a document database.
        /// Doing so let'st the querymodel cleanly encapsulate how it maintains its own state when it receives events.
        /// </summary>
        public class SingletonStatisticsQuerymodel : SelfGeneratingQueryModel<SingletonStatisticsQuerymodel, AccountEvent.Root>
        {
            public SingletonStatisticsQuerymodel()
            {
                RegisterEventAppliers()
                   .For<AccountEvent.Created>(created => NumberOfAccounts++)
                   .For<AccountEvent.LoginAttempted>(loginAttempted => NumberOfLoginsAttempts++)
                   .For<AccountEvent.LoggedIn>(loggedIn => NumberOfSuccessfulLogins++)
                   .For<AccountEvent.LoginFailed>(loginFailed => NumberOfFailedLogins++);
            }

            public int NumberOfAccounts { get; private set; }
            public int NumberOfLoginsAttempts { get; private set; }
            public int NumberOfSuccessfulLogins { get; private set; }
            public int NumberOfFailedLogins { get; private set; }

            //Since this is a singleton query model and not bound to a specific Aggregate's events we override the Id member to always be the singleton Id.
            public override Guid Id => StaticId;
            internal static Guid StaticId = Guid.Parse("93498554-5C2E-4D6A-862D-2DA7BCCAC747");
        }

        static void MaintainStatisticsWhenRelevantEventsAreReceived(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) => registrar.ForEvent(
            (AccountEvent.Root @event, ILocalApiNavigatorSession bus, StatisticsSingletonInitializer initializer) =>
            {
                initializer.EnsureInitialized(bus);

                if(new SingletonStatisticsQuerymodel().HandlesEvent(@event))
                {
                    bus.Execute(new DocumentDbApi().Queries.GetForUpdate<SingletonStatisticsQuerymodel>(SingletonStatisticsQuerymodel.StaticId))
                       .ApplyEvent(@event);
                }
            });

        internal static void Register(IEndpointBuilder builder)
        {
            builder.Container.Register(Singleton.For<StatisticsSingletonInitializer>().CreatedBy(() => new StatisticsSingletonInitializer()));
            MaintainStatisticsWhenRelevantEventsAreReceived(builder.RegisterHandlers);
        }

        class StatisticsSingletonInitializer
        {
            readonly object _initializationlock = new object();
            bool _isInitialized;
            readonly DocumentDbApi _documentDbApi = new DocumentDbApi();
            public void EnsureInitialized(ILocalApiNavigatorSession bus)
            {
                lock(_initializationlock)
                {
                    if(!_isInitialized)
                    {
                        _isInitialized = true;
                        if(bus.Execute(_documentDbApi.Queries.TryGet<SingletonStatisticsQuerymodel>(SingletonStatisticsQuerymodel.StaticId)) is None<SingletonStatisticsQuerymodel>)
                        {
                            bus.Execute(_documentDbApi.Commands.Save(new SingletonStatisticsQuerymodel()));
                        }
                    }
                }
            }
        }
    }
}
