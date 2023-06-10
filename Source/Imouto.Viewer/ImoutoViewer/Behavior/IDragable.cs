using System.Windows;

namespace ImoutoViewer.Behavior;

internal interface IDragable
{
    /// <summary>
    /// The data item.
    /// </summary>
    object? Data { get; }

    /// <summary>
    /// Allowed Drag & Drop effects.
    /// </summary>
    DragDropEffects AllowDragDropEffects { get; }
}
