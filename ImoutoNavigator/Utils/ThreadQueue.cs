using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImoutoNavigator.Utils
{
    static class ThreadQueue
    {
        private static Task _task = Task.Run(new Action(ThreadMethod));

        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static bool _isEmpty = true;

        public static void ClearQueue()
        {
            lock (_queue)
            {
                _queue.Clear();
                _isEmpty = true;
            }
        }

        public static void Add(Action action)
        {
            lock (_queue)
            {
                _queue.Enqueue(action);
                _isEmpty = false;
            }
        }

        private static Action GetFromQueue()
        {
            lock (_queue)
            {
                var result = _queue.Dequeue();
                if (!_queue.Any())
                {
                    _isEmpty = true;
                }
                return result;            
            }
        }

        private static void ThreadMethod()
        {
            while (true)
            {
                if (_isEmpty)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    GetFromQueue().Invoke();
                }
            }
        }
    }
}
