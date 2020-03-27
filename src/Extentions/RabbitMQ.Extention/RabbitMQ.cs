using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace RabbitMQ.Extention
{
    class RabbitMQ : IRabbitMQ
    {
        private List<RabbitMQConnect> rabbitMQConnects = new List<RabbitMQConnect>();

        private readonly ConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;

        private readonly ILogger<RabbitMQ> _logger;

        private readonly Queue<RabbitMQConnect> GCConnets = new Queue<RabbitMQConnect>();

        private readonly int MaxConnect = 1;

        private readonly object _objLock = new object();

        public RabbitMQ(IConfiguration configuration, ILogger<RabbitMQ> logger)
        {
            _logger = logger;

            _configuration = configuration;

            _connectionFactory = new ConnectionFactory();

            var rabbitSection = configuration.GetSection("RabbitMQ");

            var maxConnectSection = rabbitSection.GetSection("MaxConnect");
            if (maxConnectSection != null)
                MaxConnect = int.Parse(maxConnectSection.Value);

            _connectionFactory.Uri = new Uri(rabbitSection["url"]);

            var HeartbeatSection = rabbitSection.GetSection("Heartbeat");
            if (HeartbeatSection.Value != null)
                _connectionFactory.RequestedHeartbeat = ushort.Parse(HeartbeatSection.Value);

            var RecoveryIntervalSection = rabbitSection.GetSection("RecoveryIntervalRecoveryInterval");
            if (RecoveryIntervalSection.Value != null)
                _connectionFactory.NetworkRecoveryInterval = TimeSpan.Parse(RecoveryIntervalSection.Value);

            _connectionFactory.RequestedChannelMax = ushort.Parse(rabbitSection["ChannelMax"]);

            var RecoveryEnabledSection = rabbitSection.GetSection("RecoveryEnabled");
            if (RecoveryEnabledSection.Value != null)
                _connectionFactory.AutomaticRecoveryEnabled = bool.Parse(RecoveryEnabledSection.Value);

            for (int i = 0; i < MaxConnect; i++)
            {
                rabbitMQConnects.Add(CreateConnect());
            }


            Task.Run(AutoGC);

        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="message">消息内容</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        /// <param name="mimeType">MIMETYPE类型</param>
        /// <returns></returns>
        public bool Publisher(string exchange, string exchangeType, string queue, string key, byte[] message, bool durable = true, string mimeType = "text/plain")
        {
            bool bTrue = true;

            byte deliveryMode = durable ? (byte)2 : (byte)1;
            try
            {
                using (var chanl = GetConnect().CreateChanl())
                {
                    var chan = chanl.IChannl;

                    chan.ExchangeDeclare(exchange, exchangeType, durable, false);

                    IBasicProperties msg_pros = chan.CreateBasicProperties();
                    msg_pros.DeliveryMode = deliveryMode;
                    msg_pros.ContentType = mimeType;

                    chan.BasicPublish(exchange, key, msg_pros, message);
                }
            }
            catch (Exception ex)
            {
                bTrue = false;
                _logger.LogError(ex.ToString());
            }
            return bTrue;
        }


        /// <summary>
        /// 订阅获取
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="hander">处理方法</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        public void Receiver(string exchange, string exchangeType, string queue, string key, Action<object, BasicDeliverEventArgs, IModel> hander, bool durable = true)
        {
            try
            {
                IModel chan = GetConnect().CreateChanl().IChannl;

                chan.ExchangeDeclare(exchange, exchangeType, durable, false);


                chan.QueueDeclare(queue, durable, false, false);

                chan.QueueBind(queue, exchange, key);


                EventingBasicConsumer consumer = new EventingBasicConsumer(chan);

                //开始消费
                consumer.Received += (object sender, BasicDeliverEventArgs e) =>
                {

                    hander(sender, e, chan);
                };
                string consumer_tag = chan.BasicConsume(queue, false, consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw ex;
            }
        }




        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="hander">处理方法</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        /// <param name="autoAck">自动接受，true自动，false不知道</param>
        public void GetMessage(string exchange, string exchangeType, string queue, string key, Action<BasicGetResult, IModel> hander, bool durable = true, bool autoAck = false)
        {
            try
            {
                using (var chanl = GetConnect().CreateChanl())
                {
                    var chan = chanl.IChannl;

                    chan.ExchangeDeclare(exchange, exchangeType, durable, false);


                    chan.QueueDeclare(queue, durable, false, false);


                    chan.QueueBind(queue, exchange, key);

                    hander(chan.BasicGet(queue, autoAck), chan);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw ex;
            }

        }


        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        /// <returns></returns>
        public BasicGetResult GetMessage(string exchange, string exchangeType, string queue, string key, bool durable = true)
        {
            BasicGetResult result = null;
            try
            {
                using (var chanl = GetConnect().CreateChanl())
                {
                    var chan = chanl.IChannl;

                    chan.ExchangeDeclare(exchange, exchangeType, durable, false);


                    chan.QueueDeclare(queue, durable, false, false);


                    chan.QueueBind(queue, exchange, key);

                    result = chan.BasicGet(queue, true);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return result;

        }




        private RabbitMQConnect GetConnect()
        {
            int index = 0;

            int minChanl = int.MaxValue;
            for (int i = 0; i < rabbitMQConnects.Count; i++)
            {
                index = i;
                var con = rabbitMQConnects[i];

                if (minChanl > con.SemaphoreSlim.CurrentCount)
                {
                    minChanl = con.SemaphoreSlim.CurrentCount;
                }
            }

            if (!rabbitMQConnects[index].Connection.IsOpen)
            {
                lock (_objLock)
                {
                    if (!rabbitMQConnects[index].Connection.IsOpen)
                    {
                        GCConnets.Enqueue(rabbitMQConnects[index]);
                        rabbitMQConnects[index] = CreateConnect();
                    }

                }
            }

            return rabbitMQConnects[index];
        }


        private RabbitMQConnect CreateConnect()
        {
            var conn = new RabbitMQConnect(_connectionFactory);
            conn.Connection.CallbackException += Connection_CallbackException;
            conn.Connection.ConnectionBlocked += Connection_ConnectionBlocked;
            conn.Connection.ConnectionRecoveryError += Connection_ConnectionRecoveryError;
            conn.Connection.ConnectionShutdown += Connection_ConnectionShutdown;
            conn.Connection.ConnectionUnblocked += Connection_ConnectionUnblocked;
            conn.Connection.RecoverySucceeded += Connection_RecoverySucceeded;
            return conn;
        }

        private void AutoGC()
        {
            while (GCConnets.Count > 0)
            {
                Thread.Sleep(60 * 1000);
                try
                {
                    var conn = GCConnets.Dequeue();
                    if (conn.SemaphoreSlim.CurrentCount > 0)
                    {
                        GCConnets.Enqueue(conn);
                    }
                    else if (conn.Connection.IsOpen)
                    {
                        Thread.Sleep(10 * 60 * 1000);
                        if (conn.SemaphoreSlim.CurrentCount > 0)
                            GCConnets.Enqueue(conn);
                        else
                            conn.Dispose();
                    }
                    else
                    {
                        conn.Dispose();
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex.ToString());
                }
            }
        }

        private void Connection_RecoverySucceeded(object sender, EventArgs e)
        {
            _logger.LogInformation("重连成功！", sender, e);
        }

        private void Connection_ConnectionUnblocked(object sender, EventArgs e)
        {
            _logger.LogInformation("阻塞连接恢复！", sender, e);
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation(e.ClassId + "关闭成功！", sender, e);
        }

        private void Connection_ConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            _logger.LogError("连接恢复错误：" + e.Exception, sender, e);
        }

        private void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("连接阻塞：" + e.Reason, sender, e);
        }

        private void Connection_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError("回调错误：" + e.Exception.ToString(), sender, e);
        }
    }
}
