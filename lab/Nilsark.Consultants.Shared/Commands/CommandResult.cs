namespace Nilsark.Consultants.Shared.Commands
{
    public class CommandResult
    {
        public enum ExecutionStatus
        {
            Success,
            Failure
        }

        private CommandResult()
        {
        }

        private CommandResult(ExecutionStatus status)
        {
            Status = status;
        }

        public ExecutionStatus Status { get; }

        public static CommandResult Success()
        {
            return new CommandResult(ExecutionStatus.Success);
        }

        public static CommandResult Failure()
        {
            return new CommandResult(ExecutionStatus.Failure);
        }
    }
}