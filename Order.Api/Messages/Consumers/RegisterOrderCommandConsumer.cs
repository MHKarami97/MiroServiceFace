﻿using System;
using MassTransit;
using System.Net.Http;
using Newtonsoft.Json;
using Order.Api.Models;
using Order.Api.Persistence;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Messaging.InterfacesConstants.Events;
using Messaging.InterfacesConstants.Commands;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Order.Api.Hubs;

namespace Order.Api.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {
        private readonly IOptions<Setting> _settings;
        private readonly IOrderRepository _orderRepo;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHubContext<OrderHub> _hubContext;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepo, IHttpClientFactory clientFactory,
            IHubContext<OrderHub> hubContext, IOptions<Setting> settings)
        {
            _settings = settings;
            _orderRepo = orderRepo;
            _hubContext = hubContext;
            _clientFactory = clientFactory;
        }

        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            if (result.OrderId != null && result.PictureUrl != null
                                       && result.UserEmail != null && result.ImageData != null)
            {
                SaveOrder(result);

                await _hubContext.Clients.All
                    .SendAsync("UpdateOrders", "Order Created", result.OrderId);

                var client = _clientFactory.CreateClient();

                var orderDetailData =
                    await GetFacesFromFaceApiAsync(client, result.ImageData, result.OrderId);

                var faces = orderDetailData.Item1;

                var orderId = orderDetailData.Item2;

                SaveOrderDetails(orderId, faces);

                await _hubContext.Clients.All
                    .SendAsync("UpdateOrders", "Order Processed", orderId);

                await context.Publish<IOrderProcessedEvent>(new
                {
                    OrderId = orderId,
                    result.UserEmail,
                    Faces = faces,
                    result.PictureUrl
                });
            }
        }

        private async Task<Tuple<List<byte[]>, Guid>> GetFacesFromFaceApiAsync(HttpClient client, byte[] imageData,
            Guid orderId)
        {
            var byteContent = new ByteArrayContent(imageData);

            Tuple<List<byte[]>, Guid> orderDetailData;

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using (var response =
                await client
                    .PostAsync(_settings.Value.FacesApiUrl +
                               "/api/faces?orderId=" + orderId, byteContent))
            {
                var apiResponse = await response.Content.ReadAsStringAsync();

                orderDetailData = JsonConvert
                    .DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
            }

            return orderDetailData;
        }

        private void SaveOrderDetails(Guid orderId, List<byte[]> faces)
        {
            var order = _orderRepo.GetOrderAsync(orderId).Result;

            if (order != null)
            {
                order.Status = Status.Processed;

                foreach (var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };

                    order.OrderDetails.Add(orderDetail);
                }

                _orderRepo.UpdateOrder(order);
            }
        }

        private void SaveOrder(IRegisterOrderCommand result)
        {
            var order = new Models.Order
            {
                OrderId = result.OrderId,
                UserEmail = result.UserEmail,
                Status = Status.Registered,
                PictureUrl = result.PictureUrl,
                ImageData = result.ImageData
            };

            _orderRepo.RegisterOrder(order);
        }
    }
}