using System.Collections.Generic;
using System.Threading;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueue<T> where T : class
    {
        readonly Logger theLogger = new Logger("MessageQueue");
        readonly Queue<T> _queue;
        readonly object _queueLock;
        readonly AutoResetEvent _queueEvent;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public MessageQueue(int capacity = 24)
        {
            theLogger.Info(".ctor");
            _queue = new Queue<T>(capacity);
            _queueLock = new object();
            _queueEvent = new AutoResetEvent(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueEvent"></param>
        /// <param name="capacity"></param>
        public MessageQueue(AutoResetEvent queueEvent, int capacity = 24)
        {
            theLogger.Info(".ctor");
            _queue = new Queue<T>(capacity);
            _queueLock = new object();
            _queueEvent = queueEvent;
        }

        public AutoResetEvent QueueEvent => _queueEvent;


        public void Clear()
        {
            theLogger.Info("Clear()");
            lock (_queueLock)
            {
                _queueEvent.Reset();
                _queue.Clear();
            }
        }

        public void Enqueue(T message)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(message);
                _queueEvent.Set();
            }
        }
        public bool TryDequeue(out T message)
        {
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    message = null;
                    return false;
                }
                message = _queue.Dequeue();
                return true;
            }
        }
        public bool TryPeek(out T message)
        {
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    message = null;
                    return false;
                }
                message = _queue.Peek();
                return true;
            }
        }
        public void Dequeue(out T message)
        {
            lock (_queueLock)
            {
                message = _queue.Dequeue();
            }
        }

        public int Count
        {
            get
            {
                lock (_queueLock)
                {
                    return _queue.Count;
                }
            }
        }
    }
}
