using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace WP.AzureESBAdapter
{
    public class AzureESBQueue : IQueue, ICreateQueue, IDisposable, IDropQueue
    {
        private readonly IAzureESBConfiguration _configuration;
        private MessagingFactory _factory;
        private QueueClient _queueClient;
        private NamespaceManager _namespaceManager;
        private readonly int _operationRetryCount;

        private readonly AzureESBUriParser _parser;
        public AzureESBQueue(Uri uri, IAzureESBConfiguration configuration)
        {
            Guard.AgainstNull(uri, "uri");
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
            _operationRetryCount = _configuration.OperationRetryCount;

            _parser = new AzureESBUriParser(uri, configuration);

            Uri = uri;

            _factory = MessagingFactory.CreateFromConnectionString(_parser.ConnectionString);
        }

        public void Create()
        {
            EnsureQueueExists();
        }

        private void EnsureQueueExists()
        { 
            if (!namespaceManager.QueueExists(_parser.Queue))
            {
                namespaceManager.CreateQueue(_parser.Queue);
            }
        }

        private QueueClient queueClient
        {
            get
            {
                if (_queueClient != null)
                {
                    return _queueClient;
                }

                return AccessQueue(() =>
                {
                    EnsureQueueExists();
                    _queueClient = _factory.CreateQueueClient(_parser.Queue);
                    return _queueClient;
                });
            }
        }

        private NamespaceManager namespaceManager
        {
            get
            {
                if (_namespaceManager != null)
                {
                    return _namespaceManager;
                }

                return AccessQueue(() =>
                {
                    _namespaceManager = NamespaceManager.CreateFromConnectionString(_parser.ConnectionString);
                    return _namespaceManager;
                });
            }
        }

        public void Enqueue(Guid messageId, Stream stream)
        {
            AccessQueue(() =>
            {
                stream.Seek(0, SeekOrigin.Begin);
                BrokeredMessage message = new BrokeredMessage(stream);
                queueClient.Send(message);
            });
        }

        public void Drop()
        {
            AccessQueue(() => 
            {
                if (!namespaceManager.QueueExists(_parser.Queue))
                {
                    return;
                }
                namespaceManager.DeleteQueue(_parser.Queue);
            });
        }

        public Uri Uri { get; private set; }

        public bool IsEmpty()
        {
            return true;
        }
        public ReceivedMessage GetMessage()
        {
            return AccessQueue(() =>
            {
                if (queueClient.Peek() == null)
                    return null;

                BrokeredMessage message = queueClient.Receive(TimeSpan.Zero);
                if (message == null)
                    return null;

                MemoryStream ms = new MemoryStream();
                message.GetBody<Stream>().CopyTo(ms);
                ms.Position = 0;
                return new ReceivedMessage(ms, message);

            });
        }
        public void Acknowledge(object acknowledgementToken)
        {
            AccessQueue(() =>
            {
                ((BrokeredMessage)acknowledgementToken).Complete();
            });
        }

        public void Purge()
        {
            AccessQueue(() =>
            {
                while (queueClient.Peek() != null)
                {
                    var brokeredMessage = queueClient.Receive();
                    brokeredMessage.Complete();
                }
            });
        }

        public void Release(object acknowledgementToken)
        {
            AccessQueue(() =>
            {
                //Note: A received message cannot be directly sent to another entity, nor can GetBody<> be 
                //called [consumed] more than once, so we have to clone the object as shown below...
                BrokeredMessage deferredMessage = (BrokeredMessage)acknowledgementToken;
                queueClient.Send(deferredMessage.Clone());
                deferredMessage.Complete();
            });
        }

        private void AccessQueue(Action action, int retry = 0)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                if (retry == _configuration.OperationRetryCount)
                {
                    throw;
                }

                Dispose();

                AccessQueue(action, retry + 1);
            }
        }
        private T AccessQueue<T>(Func<T> action, int retry = 0)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception)
            {
                if (retry == 3)
                {
                    throw;
                }

                Dispose();

                return AccessQueue(action, retry + 1);
            }
        }

        public void Dispose()
        {
            if (_queueClient != null)
            {
                _queueClient.Close();
            }
            if (_factory != null)
            {
                _factory.Close();
            }
        }
    }
}
