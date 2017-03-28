﻿using AccountManagement.Domain.Events.PropertyUpdated;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;

namespace AccountManagement.UI.QueryModels.DocumentDB.Updaters.ContainerInstallers
{
    static class EventHandlersInstaller
    {
        internal static void Install(IDependencyInjectionContainer container)
        {
            container.Register(
                               CComponent.For<EmailToAccountMapQueryModelUpdater>()
                                         .ImplementedBy<EmailToAccountMapQueryModelUpdater>()
                                         .LifestyleScoped()
                              );

            container.CreateServiceLocator().Use<IMessageHandlerRegistrar>(
                registrar => registrar.ForEvent<IAccountEmailPropertyUpdatedEvent>(
                    @event => container.CreateServiceLocator().Use<EmailToAccountMapQueryModelUpdater>(updater => updater.Handle(@event))));
        }
    }
}
