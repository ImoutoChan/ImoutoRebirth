using System;

namespace ImoutoViewer.Model
{
    [Serializable]
    enum ResizeType
    {
        FitToViewPort,
        DownscaleToViewPort,
        FitToViewPortWidth,
        DownscaleToViewPortWidth,
        NoResize,
        Default
    }
}