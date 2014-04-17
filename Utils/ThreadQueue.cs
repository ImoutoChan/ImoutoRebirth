using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public class ThreadQueue
    {
        private Task _task;

        private readonly Queue<Action> _queue;
        //private bool _isEmpty;

        public ThreadQueue()
        {
            //_isEmpty = true;
            _queue = new Queue<Action>();
            _task = Task.Run(new Action(ThreadMethod));
        }


        public void ClearQueue()
        {
            lock (_queue)
            {
                _queue.Clear();
                //_isEmpty = true;
            }
        }

        public void Add(Action action)
        {
            lock (_queue)
            {
                _queue.Enqueue(action);
                //_isEmpty = false;
            }
        }

        private Action GetFromQueue()
        {
            lock (_queue)
            {
                var result = _queue.Dequeue();
                if (!_queue.Any())
                {
                    //_isEmpty = true;
                }
                return result;            
            }
        }

        private void ThreadMethod()
        {
            while (true)
            {
                Action action = null;

                lock (_queue)
                {
                    if (_queue.Any())
                    {
                        action = GetFromQueue();
                    }
                }


                if (action != null)
                {
                    action.Invoke();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
