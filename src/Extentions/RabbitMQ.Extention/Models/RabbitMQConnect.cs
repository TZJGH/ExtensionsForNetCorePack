using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Extention
{
    class RabbitMQConnect
    {

        public readonly SemaphoreSlim SemaphoreSlim;

        public readonly IConnection Connection;

        public RabbitMQConnect(ConnectionFactory factory)
        {
            SemaphoreSlim = new SemaphoreSlim(factory.RequestedChannelMax == 0 ? ushort.MaxValue : factory.RequestedChannelMax);

            Connection = factory.CreateConnection();
        }

        public RabbitMQChanl CreateChanl()
        {
            SemaphoreSlim.Wait();
            return new RabbitMQChanl(this);
        }


        public void Dispose()
        {
            Connection.Dispose();
        }


    }
}
