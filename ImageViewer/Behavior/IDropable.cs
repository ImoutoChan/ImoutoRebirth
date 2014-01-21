using System.Collections.Generic;

namespace ImageViewer.Behavior
{
    interface IDropable
    {
        /// <summary>
        /// Dropable types of data
        /// </summary>
        List<string> AllowDataTypes { get; }

        /// <summary>
        /// Drop data logic
        /// </summary>
        /// <param name="data">The data to be dropped</param>
        void Drop(object data);
    }
}
