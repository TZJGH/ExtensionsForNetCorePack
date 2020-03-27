using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Extention;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQExton
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQ, RabbitMQ.Extention.RabbitMQ>();
            return services;
        }


    }
}
