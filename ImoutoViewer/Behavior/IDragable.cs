using System.Windows;

namespace Imouto.Viewer.Behavior
{
    interface IDragable
    {
        /// <summary>
        /// The data item.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Allowed Drag & Drop effects.
        /// </summary>
        DragDropEffects AllowDragDropEffects { get; }
    }
}
