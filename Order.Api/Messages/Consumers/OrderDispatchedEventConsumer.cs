using System;
using MassTransit;
using Order.Api.Models;
using Order.Api.Persistence;
using System.Threading.Tasks;
using Messaging.InterfacesConstants.Events;

namespace Order.Api.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderDispatchedEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;
            
            UpdateDatabase(orderId);
            
            return Task.CompletedTask;
        }

        private void UpdateDatabase(Guid orderId)
        {
            var order = _orderRepository.GetOrder(orderId);
            
            if (order != null)
            {
                order.Status = Status.Sent;
                _orderRepository.UpdateOrder(order);
            }
        }
    }
}