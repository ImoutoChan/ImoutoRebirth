using System;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

public struct Rate
{
    public int Rating { get; }

    public Rate(int value)
    {
        if (value < 0 || value > 5)
            throw new ArgumentOutOfRangeException(nameof(value));

        Rating = value;
    }
}