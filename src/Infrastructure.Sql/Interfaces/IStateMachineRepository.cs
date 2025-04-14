using EasyP2P.Infrastructure;

namespace Infrastructure.Sql.Interfaces;
public interface IStateMachineRepository
{
    Task<StateMachine> LoadAsync(Guid id);
    Task SaveAsync(StateMachine stateMachine);
    Task SaveTransitionAsync(StateMachine stateMachine, State previousState, Trigger trigger);
}
