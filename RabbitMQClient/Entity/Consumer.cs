using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClient.Entity
{
    public class Consumer<T> : IDisposable
    {
        private readonly IModel _channel;
        private readonly bool _autoAck;
        private readonly string _queueName;
        private readonly ushort _prefetchCount;
        public event Action<T, ulong> ReceiveMessage;

        public Consumer(IModel channel, string queueName, ushort prefetchCount, bool autoAck)
        {
            this._channel = channel;
            this._autoAck = autoAck;
            this._queueName = queueName;
            this._prefetchCount = prefetchCount;
        }

        public void InitializeObject()
        {
            _channel.QueueDeclare(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var data = JsonConvert.DeserializeObject<T>(message);

                ReceiveMessage?.Invoke(data, ea.DeliveryTag);
            };

            _channel.BasicQos(0, _prefetchCount, false);

            _channel.BasicConsume(queue: _queueName, autoAck: _autoAck, consumer: consumer);
        }

        public void BasicAck(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag, false);
        }

        public void BasicNack(ulong deliveryTag, bool requeued = true)
        {
            _channel.BasicNack(deliveryTag, false, requeued);
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
                this._channel.Dispose();
            }

            this._alreadyDisposed = true;
        }

        /// <summary>
        /// Whether the object was already disposed.
        /// </summary>
        private bool _alreadyDisposed = false;
    }
}
