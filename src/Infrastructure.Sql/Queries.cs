namespace Infrastructure.Sql;

public class Queries
{
    public static string CreateStateMachinesTable =
        "CREATE TABLE state_machines " +
        "(id UUID PRIMARY KEY, " +
        "current_state INT NOT NULL, " +
        "entity_type VARCHAR(100), " +
        "entity_id VARCHAR(100), " +
        "last_transition TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
        "created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
        "updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP);";

    public static string CreateStateMachineEventsTable =
        "CREATE TABLE state_machine_events " +
        "(id UUID PRIMARY KEY," +
        "process_id UUID NOT NULL," +
        "event_type VARCHAR(100) NOT NULL," +
        "event_data JSONB NOT NULL," +
        "occurred_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP," +
        "sequence_number BIGSERIAL NOT NULL," +
        "version INT NOT NULL DEFAULT 1," +
        "FOREIGN KEY (process_id) REFERENCES state_machines (id));" +
        "-- Index for faster event retrieval by process_id" +
        "CREATE INDEX idx_state_machine_events_process_id ON state_machine_events(process_id);" +
        "-- Index for querying by event type" +
        "CREATE INDEX idx_state_machine_events_event_type ON state_machine_events(event_type);" +
        "-- Composite index for efficiently retrieving events in order" +
        "CREATE INDEX idx_state_machine_events_process_version ON state_machine_events(process_id, version);";
}
