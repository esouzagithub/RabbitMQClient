using System;
using RabbitMQ.Client;
using RabbitMQClient.Entity;
using RabbitMQClient.Library;

namespace RabbitMQClient
{
    public class QueueManager<T> : IDisposable
    {
        private readonly IModel _channel;
        private readonly Producer<T> _producer;
        private readonly Consumer<T> _consumer;
        public event Action<T, ulong> ReceiveMessage;

        public QueueManager(ConnectionConfig connectionConfig, string queueName, ushort prefetchCount = 1, bool autoAck = false)
        {
            _channel = ChannelFactory.Create(connectionConfig);

            this._producer = new Producer<T>(_channel, queueName);

            this._consumer = new Consumer<T>(_channel, queueName, prefetchCount, autoAck);

            this._consumer.ReceiveMessage += (arg1, arg2) => { ReceiveMessage?.Invoke(arg1, arg2); };
        }

        public void WatchInit()
        {
            _consumer.InitializeObject();
        }

        public void Publish(T obj)
        {
            _producer.Publish(obj);
        }

        public void Ack(ulong deliveryTag)
        {
            this._consumer.BasicAck(deliveryTag);
        }

        public void NAck(ulong deliveryTag, bool requeued = true)
        {
            this._consumer.BasicNack(deliveryTag, requeued);
        }

        /// <summary>
        /// 'IDisposable' implementation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 'IDisposable' implementation.
        /// </summary>
        /// <param name="disposeManaged">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposeManaged)
        {
            // Return if already disposed.
            if (this._alreadyDisposed) return;

            // Release managed resources if needed.
            if (disposeManaged)
            {
                this._consumer.Dispose();
                this._producer.Dispose();
                this._channel.Dispose();
                ChannelFactory.CloseConnection();
            }

            this._alreadyDisposed = true;
        }

        /// <summary>
        /// Whether the object was already disposed.
        /// </summary>
        private bool _alreadyDisposed = false;
    }
}
