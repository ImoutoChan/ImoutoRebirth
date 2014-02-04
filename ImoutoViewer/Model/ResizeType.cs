using System;

namespace ImoutoViewer.Model
{
    [Serializable]
    public enum ResizeType
    {
        FitToViewPort,
        DownscaleToViewPort,
        FitToViewPortWidth,
        DownscaleToViewPortWidth,
        NoResize,
        Default
    }
}