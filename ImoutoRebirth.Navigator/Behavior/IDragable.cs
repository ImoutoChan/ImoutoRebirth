using System.Windows;

namespace ImoutoRebirth.Navigator.Behavior
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
