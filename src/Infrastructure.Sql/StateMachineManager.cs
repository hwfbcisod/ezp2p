using Infrastructure.Sql.Interfaces;
using System;
using System.Threading.Tasks;

namespace EasyP2P.Infrastructure;

public class StateMachineManager : IStateMachineManager
{
    private readonly IStateMachineRepository _repository;

    public StateMachineManager(IStateMachineRepository repository)
    {
        _repository = repository;
    }

    public StateMachine Create(State initialState)
    {
        var stateMachine = new StateMachine(initialState);
        return stateMachine;
    }

    public async Task<StateMachine> LoadAsync(Guid id)
    {
        return await _repository.LoadAsync(id).ConfigureAwait(false);
    }

    public async Task SaveAsync(StateMachine stateMachine)
    {
        await _repository.SaveAsync(stateMachine).ConfigureAwait(false);
    }
}