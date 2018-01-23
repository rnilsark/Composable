﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Composable.Messaging;
using Composable.Refactoring.Naming;
using Composable.System;
using Composable.System.Linq;
using Composable.System.Reflection;
using JetBrains.Annotations;

namespace Composable.Persistence.EventStore.Aggregates
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class AllowPublicSettersAttribute : Attribute {}

    static class AggregateTypeValidator<TDomainClass, TEventImplementation, TEvent>
    {
        public static void AssertStaticStructureIsValid()
        {
            List<Type> typesToInspect = Seq.OfTypes<TDomainClass, TEventImplementation, TEvent>().ToList();

            typesToInspect.AddRange(GetAllInheritingClassesOrInterfaces(typeof(TDomainClass)));
            typesToInspect.AddRange(GetAllInheritingClassesOrInterfaces(typeof(TEventImplementation)));
            typesToInspect.AddRange(GetAllInheritingClassesOrInterfaces(typeof(TEvent)));

            typesToInspect = typesToInspect.Distinct().ToList();

            var illegalMembers = typesToInspect.SelectMany(GetBrokenMembers).Distinct().ToList();

            if(illegalMembers.Any())
            {
                // ReSharper disable once PossibleNullReferenceException
                var brokenMembers = illegalMembers.Select(illegal => $"{illegal.DeclaringType.FullName}.{illegal.Name}").Distinct().OrderBy(me => me).Join(Environment.NewLine);
                var message = $@"Types used by aggregate contains types that have public setters or public  fields. This is a dangerous design. 
If you ever mutate an event or an aggregate except by raising events your state is likely to become currupt in our caches etc. 
List of problem members:{Environment.NewLine}{brokenMembers}{Environment.NewLine}{Environment.NewLine}";

                Console.WriteLine(message);

                throw new Exception(message);
            }
        }

        static IEnumerable<MemberInfo> GetBrokenMembers(Type type)
        {
            var publicFields = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(member => member.MemberType.HasFlag(MemberTypes.Field)).ToList();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var publicProperties = properties
                                  .Where(member => member?.SetMethod?.IsPublic == true)
                                  .ToList();

            var totalMutableProperties = publicFields.Concat(publicProperties).ToList();
            // ReSharper disable once AssignNullToNotNullAttribute
            totalMutableProperties = totalMutableProperties.Where(member => member.DeclaringType.GetCustomAttribute<AllowPublicSettersAttribute>() == null).ToList();

            return totalMutableProperties;
        }

        static IReadOnlyList<Type> GetAllInheritingClassesOrInterfaces(Type type) => type.Assembly.GetTypes()
                                                                                         .Where(type.IsAssignableFrom)
                                                                                         .ToList();
    }

    [UsedImplicitly] class AggregateTypeValidator : IAggregateTypeValidator
    {
        readonly ITypeMapper _typeMapper;
        public AggregateTypeValidator(ITypeMapper typeMapper) => _typeMapper = typeMapper;

        public void AssertIsValid<TAggregate>() { ValidatorFor<TAggregate>.AssertValid(_typeMapper); }

        static class ValidatorFor<TAggregate>
        {
            // ReSharper disable once StaticMemberInGenericType (This is exactly the effect we are after...)
            static bool _validated;

            internal static void AssertValid(ITypeMapper typeMapper)
            {
                if(_validated) return;

                AssertValidInternal(typeMapper);

                _validated = true;
            }

            static void AssertValidInternal(ITypeMapper typeMapper)
            {
                var classInheritanceChain = typeof(TAggregate).ClassInheritanceChain().ToList();
                var inheritedAggregateType = classInheritanceChain.Where(baseClass => baseClass.IsConstructedGenericType && baseClass.GetGenericTypeDefinition() == typeof(Aggregate<,,>)).Single();

                var detectedEventImplementationType = inheritedAggregateType.GenericTypeArguments[1];
                var detectedEventType = inheritedAggregateType.GenericTypeArguments[2];

                var eventTypesToInspect = new List<Type> {detectedEventType, detectedEventImplementationType};

                eventTypesToInspect.AddRange(GetAllInheritingClassesOrInterfaces(detectedEventImplementationType));
                eventTypesToInspect.AddRange(GetAllInheritingClassesOrInterfaces(detectedEventType));

                eventTypesToInspect = eventTypesToInspect.Distinct().ToList();

                typeMapper.AssertMappingsExistFor(eventTypesToInspect.Append(typeof(TAggregate)));

                MessageInspector.AssertValid(eventTypesToInspect);
            }

            static IReadOnlyList<Type> GetAllInheritingClassesOrInterfaces(Type type) => type.Assembly.GetTypes()
                                                                                             .Where(type.IsAssignableFrom)
                                                                                             .ToList();
        }
    }

    static class MessageInspector
    {
        static readonly HashSet<Type> SuccessfullyInspectedTypes = new HashSet<Type>();

        internal static void AssertValid(IReadOnlyList<Type> eventTypesToInspect)
        {
            foreach(var type in eventTypesToInspect)
            {
                AssertTypeIsValid(type);
            }
        }

        public static void AssertValidToSend(IMessage message)
        {
            if(message is IRequiresTransactionalSendOperationMessage && Transaction.Current == null)
            {
                throw new Exception($"{message.GetType().FullName} is {nameof(IRequiresTransactionalSendOperationMessage)} but there is no transaction.");
            }

            if(message is IForbidTransactionalSendOperationMessage && Transaction.Current != null)
            {
                throw new Exception($"{message.GetType().FullName} is {nameof(IForbidTransactionalSendOperationMessage)} but there is a transaction.");
            }

            AssertTypeIsValid(message.GetType());
        }

        static void AssertTypeIsValid(Type type)
        {
            lock(SuccessfullyInspectedTypes)
            {
                if(SuccessfullyInspectedTypes.Contains(type)) return;

                if(type.Implements<ICommand>() && type.Implements<IEvent>())
                {
                    throw new Exception($"{type.FullName} implements both {typeof(ICommand)} and {typeof(IEvent)}.");
                }

                if(type.Implements<ICommand>() && type.Implements<IQuery>())
                {
                    throw new Exception($"{type.FullName} implements both {typeof(ICommand)} and {typeof(IQuery)}.");
                }

                if(type.Implements<IEvent>() && type.Implements<IQuery>())
                {
                    throw new Exception($"{type.FullName} implements both {typeof(IEvent)} and {typeof(IQuery)}.");
                }

                if(type.Implements<IRemoteMessage>() && type.Implements<ILocalMessage>())
                {
                    throw new Exception($"{type.FullName} implements both {typeof(IRemoteMessage)} and {typeof(ILocalMessage)}.");
                }

                if(type.Implements<IRequiresTransactionalSendOperationMessage>() && type.Implements<IForbidTransactionalSendOperationMessage>())
                {
                    throw new Exception($"{type.FullName} implements both {typeof(IRequiresTransactionalSendOperationMessage)} and {typeof(IForbidTransactionalSendOperationMessage)}.");
                }
                SuccessfullyInspectedTypes.Add(type);
            }
        }
    }
}
