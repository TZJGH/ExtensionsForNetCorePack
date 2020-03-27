using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace RabbitMQ.Extention
{
    class RabbitMQChanl : IDisposable
    {
        private readonly RabbitMQConnect _rabbitMQConnect;

        public IModel IChannl { get; private set; }
        public RabbitMQChanl(RabbitMQConnect rabbitMQConnect)
        {
            _rabbitMQConnect = rabbitMQConnect;
            IChannl = _rabbitMQConnect.Connection.CreateModel();

        }

        public void Dispose()
        {
            if (IChannl != null)
                IChannl.Dispose();
            _rabbitMQConnect.SemaphoreSlim.Release();

        }
    }
}
