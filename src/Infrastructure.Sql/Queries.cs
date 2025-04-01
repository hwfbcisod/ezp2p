using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
