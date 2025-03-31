using System;

namespace EasyP2P.Infrastructure.Exceptions;
internal class TransitionNotFoundException : Exception
{
    public TransitionNotFoundException(string message) : base(message)
    {
        
    }
}
