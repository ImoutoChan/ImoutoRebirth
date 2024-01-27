namespace ImoutoViewer.Behavior;

internal interface IDropable
{
    /// <summary>
    /// Dropable types of data.
    /// </summary>
    List<string> AllowDataTypes { get; }

    /// <summary>
    /// Drop data logic.
    /// </summary>
    /// <param name="data">The data to be dropped.</param>
    /// <param name="sourceGuid">Drag source</param>
    void Drop(object data, object? sourceGuid);
}
