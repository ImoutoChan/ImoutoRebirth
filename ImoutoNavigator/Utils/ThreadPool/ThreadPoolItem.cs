using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ImoutoNavigator.Utils.ThreadPool
{
    internal class ThreadPoolItem
    {
        #region Fileds

        private readonly Action _callbackAct;
        private readonly Action _threadAct;
        private bool _isBusy;

        #endregion //Fields

        #region Constructors

        public ThreadPoolItem()
        {
            WorkAction = () => { };
            _callbackAct += ThreadCallback;
            _threadAct += ThreadAction;

            Recreate();
        }

        #endregion //Constructors

        #region Properties

        public Thread ThreadItem { get; set; }

        public int WorkActionId { get; set; }

        public Action WorkAction { get; set; }

        public Action WorkCallback { get; set; }

        public bool IsBusy
        {
            get
            {
                lock (this)
                {
                    return _isBusy;
                }
            }
            set
            {
                lock (this)
                {
                    _isBusy = value;
                }
            }
        }

        #endregion //Properties

        #region Public methods

        public void Abort()
        {
            try
            {
                lock (this)
                {
                    ThreadItem.Abort();
                }
            }
            catch (Exception)
            { }
            finally
            {
                IsBusy = false;
            }
        }

        public void Run(Action action, Action callback, int id, string threadName = "")
        {
            DateTime startTime = DateTime.Now;
            lock (this)
            {
                Recreate();
                IsBusy = true;

                Debug.Print("!RUN!RECREATED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

                WorkAction = null;
                WorkAction = action;

                WorkCallback = null;
                WorkCallback = callback;

                WorkActionId = id;

                Debug.Print("!RUN!INITED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

                if (!string.IsNullOrEmpty(threadName))
                {
                    ThreadItem.Name = threadName;
                }

                Debug.Print("!RUN!NAMED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);

                ThreadItem.Start();

                Debug.Print("!RUN!STARTED AT {0}\t{1}", (DateTime.Now - startTime).TotalMilliseconds, DateTime.Now.Millisecond);
            }
        }

        #endregion //Public methods

        #region Methods

        private void ThreadCallback()
        {
            WorkCallback();

            lock (this)
            {
                IsBusy = false;
                OnThreadFinishedAsyns();
            }
        }

        private void ThreadAction()
        {
            WorkAction();

            _callbackAct();
        }

        private void Recreate()
        {
            Abort();
            ThreadItem = new Thread(new ThreadStart(_threadAct)) { IsBackground = true };
        }

        public override string ToString()
        {
            return String.Format("Is busy: {1}, Thread status: {2}", IsBusy, ThreadItem.ThreadState);
        }

        #endregion //Methods

        #region Events

        public event EventHandler ThreadFinished;

        private void OnThreadFinished()
        {
            var handler = ThreadFinished;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnThreadFinishedAsyns()
        {
            var handler = ThreadFinished;

            foreach (var deleg in handler.GetInvocationList().OfType<EventHandler>().Select(@delegate => @delegate as EventHandler))
            {
                deleg.BeginInvoke(this, new EventArgs(), null, null);
            }
        }

        #endregion //Events
    }
}