using Refit;
using System;
using System.Net;
using Web.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Web.RestClients
{
    public class OrderManagementApi : IOrderManagementApi
    {
        private readonly IOrderManagementApi _restClient;

        public OrderManagementApi(IConfiguration config, HttpClient httpClient)
        {
            var apiHostAndPort = config
                .GetSection("ApiServiceLocations")
                .GetValue<string>("OrdersApiLocation");

            httpClient.BaseAddress = new Uri($"http://{apiHostAndPort}/api");

            _restClient = RestService.For<IOrderManagementApi>(httpClient);
        }

        public async Task<OrderViewModel> GetOrderById(Guid orderId)
        {
            try
            {
                return await _restClient.GetOrderById(orderId);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<List<OrderViewModel>> GetOrders()
        {
            return await _restClient.GetOrders();
        }
    }
}