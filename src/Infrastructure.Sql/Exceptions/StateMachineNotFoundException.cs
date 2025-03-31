namespace Infrastructure.Sql.Exceptions;
public class StateMachineNotFoundException : Exception
{
    public StateMachineNotFoundException(string message) : base(message)
    {
        
    }
}
