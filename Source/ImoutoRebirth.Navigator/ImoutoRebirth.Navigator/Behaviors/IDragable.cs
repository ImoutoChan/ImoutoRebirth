using System.Windows;

namespace ImoutoRebirth.Navigator.Behaviors;

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