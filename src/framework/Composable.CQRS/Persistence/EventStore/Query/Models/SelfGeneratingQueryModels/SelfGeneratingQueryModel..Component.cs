﻿using Composable.Messaging.Events;

namespace Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels
{
    public abstract partial class SelfGeneratingQueryModel<TAggregate,  TAggregateEvent>
        where TAggregate : SelfGeneratingQueryModel<TAggregate,  TAggregateEvent>
        where TAggregateEvent : class, IAggregateRootEvent
    {
        public abstract partial class Component<TComponent, TComponentEvent>
            where TComponentEvent : class, TAggregateEvent
            where TComponent : Component<TComponent, TComponentEvent>
        {
            readonly CallMatchingHandlersInRegistrationOrderEventDispatcher<TComponentEvent> _eventAppliersEventDispatcher =
                new CallMatchingHandlersInRegistrationOrderEventDispatcher<TComponentEvent>();

            void ApplyEvent(TComponentEvent @event)
            {
                _eventAppliersEventDispatcher.Dispatch(@event);
            }

            protected Component(TAggregate aggregateRoot)
                : this(
                    appliersRegistrar: aggregateRoot.RegisterEventAppliers(),
                    registerEventAppliers: true)
            {}

            internal Component(IEventHandlerRegistrar<TComponentEvent> appliersRegistrar, bool registerEventAppliers)
            {
                if(registerEventAppliers)
                {
                    appliersRegistrar
                                 .For<TComponentEvent>(ApplyEvent);
                }
            }

            protected IEventHandlerRegistrar<TComponentEvent> RegisterEventAppliers() => _eventAppliersEventDispatcher.Register();
        }
    }
}
