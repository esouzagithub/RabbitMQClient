using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQClient.Entity;

namespace RabbitMQClient.UnitTest
{
    [TestClass]
    public class QueueManagerTests
    {

        private QueueManager<Aluno> _queueManager;
        private Aluno _expected;
        private Aluno _actual;

        [TestInitialize]
        public void TestInitialize()
        {
            var connectionconfig = new ConnectionConfig()
            {
                HostName = "127.0.0.1",
                Password = "guest",
                UserName = "guest",
                VirtualHost = "/"
            };

            _queueManager = new QueueManager<Aluno>(connectionconfig, "queueTest", 100);

            _queueManager.ReceiveMessage += (aluno, deliveryTag) =>
            {
                _expected = aluno;
                _queueManager.Ack(deliveryTag);
            };

            _queueManager.WatchInit();

            _actual = new Aluno()
            {
                Nome = "Edu",
                Matricula = $"{123456}"
            };

            _queueManager.Publish(_actual);
            Thread.Sleep(100);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _queueManager.Dispose();
        }

        [TestMethod]
        public void Publish()
        {
            Assert.AreEqual(_expected.Matricula, _actual.Matricula);
            Assert.AreEqual(_expected.Nome, _actual.Nome);
            Assert.IsInstanceOfType(_expected, typeof(Aluno));
        }
    }
}
