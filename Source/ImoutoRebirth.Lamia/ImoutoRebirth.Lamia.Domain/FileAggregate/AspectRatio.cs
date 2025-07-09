using System.Diagnostics;

namespace ImoutoRebirth.Lamia.Domain.FileAggregate;

[DebuggerDisplay("{Orientation} {Standard}/{Custom}")]
public record AspectRatio(AspectRatioType Type, StandardAspectRatio Standard, decimal? Custom, Orientation Orientation);

public enum AspectRatioType
{
    Standard,
    Custom
}

public enum StandardAspectRatio
{
    Other,

    _1x1,   // 1.00
    _19x16, // 1.19
    _5x4,   // 1.25
    _4x3,   // 1.33
    _3x2,   // 1.5
    _14x9,  // 1.56
    _16x10, // 1.60
    _5x3,   // 1.66
    _16x9,  // 1.78
            _1_85,
            _1_90,
            _2_00,
    _13x6,  // 2.17
            _2_35,
            _2_37,
            _2_39,
            _2_40,
            _2_44,
}

public enum Orientation
{
    Horizontal,
    Vertical,
    Square
}
