namespace TinyFeet.Interfaces.Command
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<ICommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
