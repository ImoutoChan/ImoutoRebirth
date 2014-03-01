using System;

namespace ImoutoNavigator.Utils.ThreadPool
{
    internal class ThreadPoolQueueItem
    {
        #region Constructors

        public ThreadPoolQueueItem(int id, Action workAction, Action callback)
        {
            WorkId = id;
            WorkAction = workAction;
            WorkCallback = callback;
            IsCanceled = false;
        }

        #endregion Constructors

        #region Properties

        public int WorkId { get; private set; }

        public Action WorkCallback { get; private set; }

        public Action WorkAction { get; private set; }

        public bool IsCanceled { get; set; }

        #endregion //Properties
    }
}