using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitMQClient.Entity
{
    public class Producer<T> : IDisposable
    {
        private readonly IModel _channel;
        private readonly string _queueName;

        public Producer(IModel channel, string queueName)
        {
            this._channel = channel;
            this._queueName = queueName;
        }

        public void Publish(T obj)
        {
            var data = JsonConvert.SerializeObject(obj);

            var buffer = Encoding.UTF8.GetBytes(data);

            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: buffer);
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
