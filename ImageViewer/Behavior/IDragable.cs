using System;
using System.Windows;

namespace ImageViewer.Behavior
{
    interface IDragable
    {
        /// <summary>
        /// Type of the data item
        /// </summary>
        string DataType { get; }

        /// <summary>
        /// The data item
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Allowed Drag & Drop effects
        /// </summary>
        DragDropEffects AllowDragDropEffects { get; }
    }
}
