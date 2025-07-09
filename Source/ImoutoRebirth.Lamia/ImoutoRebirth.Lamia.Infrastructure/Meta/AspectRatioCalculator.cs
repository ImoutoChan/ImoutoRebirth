using ImoutoRebirth.Lamia.Domain.FileAggregate;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta;

internal static class AspectRatioCalculator
{
    private static readonly Dictionary<(int, int), StandardAspectRatio> StandardLookup = new()
    {
        { (1, 1), StandardAspectRatio._1x1 },
        { (4, 3), StandardAspectRatio._4x3 },
        { (5, 4), StandardAspectRatio._5x4 },
        { (3, 2), StandardAspectRatio._3x2 },
        { (14, 9), StandardAspectRatio._14x9 },
        { (5, 3), StandardAspectRatio._5x3 },
        { (16, 10), StandardAspectRatio._16x10 },
        { (16, 9), StandardAspectRatio._16x9 },
        { (13, 6), StandardAspectRatio._13x6 },
        { (19, 16), StandardAspectRatio._19x16 }
    };

    private static readonly Dictionary<decimal, StandardAspectRatio> CloseToStandardLookup = new()
    {
        { 1.00m, StandardAspectRatio._1x1 },
        { 1.19m, StandardAspectRatio._19x16 },
        { 1.25m, StandardAspectRatio._5x4 },
        { 1.33m, StandardAspectRatio._4x3 },
        { 1.50m, StandardAspectRatio._3x2 },
        { 1.56m, StandardAspectRatio._14x9 },
        { 1.60m, StandardAspectRatio._16x10 },
        { 1.66m, StandardAspectRatio._5x3 },
        { 1.78m, StandardAspectRatio._16x9 },
        { 1.85m, StandardAspectRatio._1_85 },
        { 1.90m, StandardAspectRatio._1_90 },
        { 2.00m, StandardAspectRatio._2_00 },
        { 2.17m, StandardAspectRatio._13x6 },
        { 2.35m, StandardAspectRatio._2_35 },
        { 2.37m, StandardAspectRatio._2_37 },
        { 2.39m, StandardAspectRatio._2_39 },
        { 2.40m, StandardAspectRatio._2_40 },
        { 2.44m, StandardAspectRatio._2_44 }
    };

    public static AspectRatio? DetermineAspectRatio(Resolution resolution)
    {
        var (width, height) = resolution;

        if (width == 0 || height == 0)
            return null;

        var orientation = width == height
            ? Orientation.Square
            : width > height
                ? Orientation.Horizontal
                : Orientation.Vertical;

        var divisor = GreatestCommonDivisor(width, height);
        var aspectWidth = width / divisor;
        var aspectHeight = height / divisor;

        var left = Math.Max(aspectWidth, aspectHeight);
        var right = Math.Min(aspectWidth, aspectHeight);

        if (StandardLookup.TryGetValue((left, right), out var result))
            return new(AspectRatioType.Standard, result, null, orientation);

        var ratio = (decimal)left / right;

        foreach (var (key, value) in CloseToStandardLookup)
        {
            if (Math.Abs(key - ratio) <= 0.02m)
                return new(AspectRatioType.Standard, value, null, orientation);
        }

        return new(AspectRatioType.Custom, StandardAspectRatio.Other, Math.Round(ratio, 2), orientation);
    }

    private static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }
}
