using System;
using RabbitMQ.Client;
using RabbitMQClient.Entity;
using RabbitMQClient.Library;

namespace RabbitMQClient
{
    public class QueueManager<T> : IDisposable
    {
        private IModel _channel;
        public Producer<T> Producer { get; set; }
        public Consumer<T> Consumer { get; set; }
        private readonly string _queueName;

        public QueueManager(string queueName)
        {
            _queueName = queueName;
        }

        public QueueManager<T> WithConnectionSetting(ConnectionSetting connectionSetting)
        {
            _channel = ChannelFactory.Create(connectionSetting);

            return this;
        }

        public QueueManager<T> WithConsumer(ushort prefetchCount = 1, bool autoAck = false) {

            this.Consumer = new Consumer<T>(_channel, _queueName, prefetchCount, autoAck);

            return this;
        }

        public QueueManager<T> WithProducer() {

            this.Producer = new Producer<T>(_channel, _queueName);
            return this;
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
                this.Consumer?.Dispose();
                this.Producer?.Dispose();
                this._channel?.Dispose();
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
