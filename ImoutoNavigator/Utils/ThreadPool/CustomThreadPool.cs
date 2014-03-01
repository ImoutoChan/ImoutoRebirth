using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ImoutoNavigator.Utils.ThreadPool
{
    class CustomThreadPool
    {
        #region Fields

        private readonly List<ThreadPoolItem> _threadPoolItems;
        private readonly Queue<ThreadPoolQueueItem> _queue;
        private int _actionKeys = -1;
        private readonly Action _tryStartDelegate;
        
        #endregion //Fields

        #region Constructors

        public CustomThreadPool(int maxThreads = 10)
        {
            if (maxThreads < 1)
            {
                throw new ArgumentException("Invalid maxThreads count. It must be greater than zero.");
            }

            _threadPoolItems = new List<ThreadPoolItem>();
            _queue = new Queue<ThreadPoolQueueItem>();

            for (int i = 0; i < maxThreads; i++)
            {
                var threadItem = new ThreadPoolItem();
                threadItem.ThreadFinished += ThreadItemOnThreadFinished;
                _threadPoolItems.Add(threadItem);
            }

            _tryStartDelegate = TryStart;
        }

        #endregion //Constructors

        #region Methods

        private void TryStart()
        {
            DateTime startTime = DateTime.Now;

            ThreadPoolItem threadPoolItem;

            ThreadPoolQueueItem item;
            lock (_queue)
            {

                if (!_queue.Any()) { return; }

                threadPoolItem = _threadPoolItems.FirstOrDefault(x => !x.IsBusy);
                if (threadPoolItem == null) { return; }

                item = _queue.Dequeue();
            }

            Debug.Print("!TRYSTART!DEQUEUED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

            threadPoolItem.Run(
                item.WorkAction,
                item.WorkCallback,
                item.WorkId,
                String.Format("Loading #{0}", item.WorkId)
                );


            Debug.Print("!TRYSTART!RUNNED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);
        }

        #endregion //Methods

        #region Public methods

        public int Add(Action action, Action callback)
        {
            DateTime startTime = DateTime.Now;

            lock (_queue)
            {
                _queue.Enqueue(new ThreadPoolQueueItem(++_actionKeys, action, callback));
            }

            Debug.Print("!ADD!ENQUEUED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

            TryStart();

            Debug.Print("!ADD!TRYSTARTED AT {0}\t{1}\n", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

            return _actionKeys;
        }

        public void TryAbortOrDequeue(int actionKey)
        {
            lock (_queue)
            {
                var queueElement = _queue.FirstOrDefault(x => x.WorkId == actionKey);
                if (queueElement != null)
                {
                    queueElement.IsCanceled = true;
                    return;
                }
            }

            var threadPoolElement = _threadPoolItems.FirstOrDefault(x => x.WorkActionId == actionKey);
            if (threadPoolElement != null)
            {
                threadPoolElement.Abort();
                return;
            }
        }

        public void AbortAndDequeueAll()
        {
            lock (_queue)
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue();
                }
            }

            foreach (var threadPoolItem in _threadPoolItems)
            {
                threadPoolItem.Abort();
            }
        }

        #endregion //Public methods

        #region Event handlers

        private void ThreadItemOnThreadFinished(object sender, EventArgs eventArgs)
        {
            TryStart();
        }

        #endregion //Event handlers
    }
}
