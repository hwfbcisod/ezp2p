using EasyP2P.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Sql.Interfaces;

// The purpose of this manager is to have a single place where
// creating, loading and saving a state machine can occur.
// This makes configuring dependency injection much easier.
// Otherwise, creation must happen with a new() instantiation
// whereas Load() and Save() will be part of the repository.
public interface IStateMachineManager
{
    StateMachine Create(State initialState);
    Task<StateMachine> LoadAsync(Guid id);
    Task SaveAsync(StateMachine stateMachine);

}
