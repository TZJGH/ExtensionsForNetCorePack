using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Extention
{
    public interface IRabbitMQ
    {
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
        bool Publisher(string exchange, string exchangeType, string queue, string key, byte[] message, bool durable = true, string mimeType = "text/plain");


        /// <summary>
        /// 订阅获取
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="hander">处理方法</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        void Receiver(string exchange, string exchangeType, string queue, string key, Action<object, BasicDeliverEventArgs, IModel> hander, bool durable = true);


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
        void GetMessage(string exchange, string exchangeType, string queue, string key, Action<BasicGetResult, IModel> hander, bool durable = true, bool autoAck = false);


        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="exchange">交换器名称</param>
        /// <param name="exchangeType">交换器类型</param>
        /// <param name="queue">队列名称</param>
        /// <param name="key">绑定的键值</param>
        /// <param name="durable">是否持久 true持久，false不持久</param>
        /// <returns></returns>
        BasicGetResult GetMessage(string exchange, string exchangeType, string queue, string key, bool durable = true);
    }
}
