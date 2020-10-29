using System;

namespace Messaging.InterfacesConstants.Events
{
    public interface IOrderRegisteredEvent
    {
        Guid OrderId { get; }
    }
}