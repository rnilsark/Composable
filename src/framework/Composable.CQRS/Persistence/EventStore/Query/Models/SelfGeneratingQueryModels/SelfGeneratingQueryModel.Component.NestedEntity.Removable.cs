﻿using Composable.Messaging.Events;
using Composable.Persistence.EventStore.AggregateRoots;

namespace Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels
{
    public abstract partial class SelfGeneratingQueryModel<TAggregateRoot, TAggregateRootBaseEventClass, TAggregateRootBaseEventInterface>
        where TAggregateRoot : SelfGeneratingQueryModel<TAggregateRoot, TAggregateRootBaseEventClass, TAggregateRootBaseEventInterface>
        where TAggregateRootBaseEventInterface : class, IAggregateRootEvent
        where TAggregateRootBaseEventClass : AggregateRootEvent, TAggregateRootBaseEventInterface
    {
        public abstract partial class Component<TComponent, TComponentBaseEventClass, TComponentBaseEventInterface>
            where TComponentBaseEventInterface : class, TAggregateRootBaseEventInterface
            where TComponentBaseEventClass : TAggregateRootBaseEventClass, TComponentBaseEventInterface
            where TComponent : Component<TComponent, TComponentBaseEventClass, TComponentBaseEventInterface>
        {
            internal abstract class NestedEntity<TEntity,
                                               TEntityId,
                                               TEntityBaseEventClass,
                                               TEntityBaseEventInterface,
                                               TEntityCreatedEventInterface,
                                               TEntityRemovedEventInterface,
                                               TEventEntityIdSetterGetter> :
                                                   NestedEntity<TEntity,
                                                       TEntityId,
                                                       TEntityBaseEventClass,
                                                       TEntityBaseEventInterface,
                                                       TEntityCreatedEventInterface,
                                                       TEventEntityIdSetterGetter>
                where TEntityBaseEventInterface : class, TComponentBaseEventInterface
                where TEntityBaseEventClass : TComponentBaseEventClass, TEntityBaseEventInterface
                where TEntityCreatedEventInterface : TEntityBaseEventInterface
                where TEntityRemovedEventInterface : TEntityBaseEventInterface
                where TEventEntityIdSetterGetter :
                    IGetAggregateRootEntityEventEntityId<TEntityBaseEventInterface, TEntityId>, new()
                where TEntity : NestedEntity<TEntity,
                                    TEntityId,
                                    TEntityBaseEventClass,
                                    TEntityBaseEventInterface,
                                    TEntityCreatedEventInterface,
                                    TEventEntityIdSetterGetter>
            {
                protected NestedEntity(TComponent parent) : this(parent.RegisterEventAppliers())
                {
                }

                NestedEntity(IEventHandlerRegistrar<TEntityBaseEventInterface> appliersRegistrar): base(appliersRegistrar)
                {
                    RegisterEventAppliers()
                        .IgnoreUnhandled<TEntityRemovedEventInterface>();
                }

                internal new static CollectionManager CreateSelfManagingCollection(TComponent parent) =>
                        new CollectionManager(parent: parent, appliersRegistrar: parent.RegisterEventAppliers());

                internal new class CollectionManager : QueryModelEntityCollectionManager<TComponent,
                                                         TEntity,
                                                         TEntityId,
                                                         TEntityBaseEventClass,
                                                         TEntityBaseEventInterface,
                                                         TEntityCreatedEventInterface,
                                                         TEntityRemovedEventInterface,
                                                         TEventEntityIdSetterGetter>
                {
                    internal CollectionManager
                        (TComponent parent, IEventHandlerRegistrar<TEntityBaseEventInterface> appliersRegistrar) : base(parent, appliersRegistrar) {}
                }
            }
        }
    }
}
