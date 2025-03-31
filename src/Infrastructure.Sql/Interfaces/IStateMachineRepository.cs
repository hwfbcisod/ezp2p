using EasyP2P.Infrastructure;
using Infrastructure.Sql.Exceptions;
using Npgsql;

namespace Infrastructure.Sql.Interfaces;
public interface IStateMachineRepository
{
    Task<StateMachine> LoadAsync(Guid id);
    Task SaveAsync(StateMachine stateMachine);
    Task SaveTransitionAsync(StateMachine stateMachine, State previousState, Trigger trigger);
}

public class PostgresStateMachineRepository : IStateMachineRepository
{
    private readonly string _connectionString;

    public PostgresStateMachineRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<StateMachine> LoadAsync(Guid id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = new NpgsqlCommand(
            "SELECT current_state FROM state_machines WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("id", id);

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

        if (await reader.ReadAsync().ConfigureAwait(false))
        {
            
            int stateValue = reader.GetInt32(0);
            State currentState = (State)stateValue;

            var machine = new StateMachine(id, currentState);

            return machine;
        }

        throw new StateMachineNotFoundException($"State machine with id {id} not found");
    }

    public async Task SaveAsync(StateMachine stateMachine)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using (var checkCommand = new NpgsqlCommand(
            "SELECT 1 FROM state_machines WHERE id = @id",
            connection))
        {
            checkCommand.Parameters.AddWithValue("id", stateMachine.Id);
            var exists = await checkCommand.ExecuteScalarAsync().ConfigureAwait(false) != null;

            if (exists)
            {
                using var updateCommand = new NpgsqlCommand(
                    @"UPDATE state_machines 
                          SET current_state = @state, updated_at = @timestamp 
                          WHERE id = @id",
                    connection);

                updateCommand.Parameters.AddWithValue("id", stateMachine.Id);
                updateCommand.Parameters.AddWithValue("state", (int)stateMachine.CurrentState);
                updateCommand.Parameters.AddWithValue("timestamp", DateTime.UtcNow);

                await updateCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            else
            {
                using var insertCommand = new NpgsqlCommand(
                    @"INSERT INTO state_machines 
                          (id, current_state, created_at, updated_at) 
                          VALUES (@id, @state, @timestamp, @timestamp)",
                    connection);

                var timestamp = DateTime.UtcNow;

                insertCommand.Parameters.AddWithValue("id", stateMachine.Id);
                insertCommand.Parameters.AddWithValue("state", (int)stateMachine.CurrentState);
                insertCommand.Parameters.AddWithValue("timestamp", timestamp);

                await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }

    public async Task SaveTransitionAsync(StateMachine stateMachine, State previousState, Trigger trigger)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            using (var updateCommand = new NpgsqlCommand(
                @"UPDATE state_machines 
                      SET current_state = @state, 
                          last_transition = @timestamp,
                          updated_at = @timestamp 
                      WHERE id = @id",
                connection, transaction))
            {
                var timestamp = DateTime.UtcNow;

                updateCommand.Parameters.AddWithValue("id", stateMachine.Id);
                updateCommand.Parameters.AddWithValue("state", (int)stateMachine.CurrentState);
                updateCommand.Parameters.AddWithValue("timestamp", timestamp);

                await updateCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            using (var historyCommand = new NpgsqlCommand(
                @"INSERT INTO state_machine_history 
                      (id, state_machine_id, from_state, to_state, trigger, transition_time) 
                      VALUES (@id, @machineId, @fromState, @toState, @trigger, @timestamp)",
                connection, transaction))
            {
                var timestamp = DateTime.UtcNow;

                historyCommand.Parameters.AddWithValue("id", Guid.NewGuid());
                historyCommand.Parameters.AddWithValue("machineId", stateMachine.Id);
                historyCommand.Parameters.AddWithValue("fromState", (int)previousState);
                historyCommand.Parameters.AddWithValue("toState", (int)stateMachine.CurrentState);
                historyCommand.Parameters.AddWithValue("trigger", (int)trigger);
                historyCommand.Parameters.AddWithValue("timestamp", timestamp);

                await historyCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }
}

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
