using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common.CQRS;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Common.ServiceFabric.Extensions
{
    [DataContract]
    internal sealed class CommandReturnValue
    {
        private CommandReturnValue()
        {
        }

        private CommandReturnValue(object returnValue)
        {
            ReturnValue = returnValue;
        }

        [DataMember]
        public object ReturnValue { get; set; }

        [IgnoreDataMember]
        public bool HasReturnValue => ReturnValue != null;

        public static CommandReturnValue None()
        {
            return new CommandReturnValue();
        }

        public static CommandReturnValue Create(object returnValue)
        {
            return new CommandReturnValue(returnValue);
        }
    }

    /// <summary>
    ///     Message deduplication helper can be used when it is not possible to achieve natural idempotency.
    /// </summary>
    public static class CommandDeduplicationHelper
    {
        private const string CommandStateKeyPrefix = "__command_";

        public static async Task<TReturnValue> ProcessOnceAsync<TReturnValue>
        (Func<CancellationToken, Task<TReturnValue>> func, ICommand command, IActorStateManager stateManager,
            CancellationToken cancellationToken)
            where TReturnValue : struct
        {
            if (await HasPreviousExecution(command, stateManager, cancellationToken))
            {
                var conditionalValue =
                    await stateManager.TryGetStateAsync<CommandReturnValue>(GetStateKey(command), cancellationToken);

                if (conditionalValue.HasValue && conditionalValue.Value.HasReturnValue)
                    return (TReturnValue) conditionalValue.Value.ReturnValue;

                return default(TReturnValue);
            }

            var returnValue = await func(cancellationToken);
            await StoreCommandAndReturnValue(command, stateManager, CommandReturnValue.Create(returnValue),
                cancellationToken);
            return returnValue;
        }

        public static async Task ProcessOnceAsync
        (Func<CancellationToken, Task> func, ICommand command, IActorStateManager stateManager,
            CancellationToken cancellationToken)
        {
            if (await HasPreviousExecution(command, stateManager, cancellationToken))
                return;

            await func(cancellationToken);
            await StoreCommand(command, stateManager, cancellationToken);
        }

        public static async Task ProcessOnceAsync
            (Action action, ICommand command, IActorStateManager stateManager, CancellationToken cancellationToken)
        {
            if (await HasPreviousExecution(command, stateManager, cancellationToken))
                return;

            action();
            await StoreCommand(command, stateManager, cancellationToken);
        }

        private static async Task<bool> HasPreviousExecution(ICommand command, IActorStateManager stateManager,
            CancellationToken cancellationToken)
        {
            return await stateManager.ContainsStateAsync(GetStateKey(command), cancellationToken);
        }

        private static Task StoreCommand(ICommand command, IActorStateManager stateManager,
            CancellationToken cancellationToken)
        {
            return StoreCommandAndReturnValue(command, stateManager, CommandReturnValue.None(), cancellationToken);
        }

        private static Task StoreCommandAndReturnValue(ICommand command, IActorStateManager stateManager,
            CommandReturnValue returnValue, CancellationToken cancellationToken)
        {
            return stateManager.AddStateAsync(GetStateKey(command), returnValue, cancellationToken);
        }

        private static string GetStateKey(ICommand command)
        {
            return $"{CommandStateKeyPrefix}{command.CommandId}";
        }
    }
}