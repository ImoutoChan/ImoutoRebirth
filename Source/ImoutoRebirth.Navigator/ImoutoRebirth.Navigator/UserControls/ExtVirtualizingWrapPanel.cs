namespace ImoutoRebirth.Navigator.UserControls;

public class ExtVirtualizingWrapPanel : WpfToolkit.Controls.VirtualizingWrapPanel
{
    protected override double GetLineUpScrollAmount()
    {
        var itemHeight = -base.GetLineUpScrollAmount();

        var rem = Mod(VerticalOffset, itemHeight);

        if (rem < 0.5)
            rem = 0;

        var step = rem > 0 ? rem : itemHeight;

        return -Math.Min(step, ViewportHeight);
    }

    protected override double GetLineDownScrollAmount()
    {
        var itemHeight = base.GetLineDownScrollAmount();

        var bottomOffset = VerticalOffset + ViewportHeight;
        var rem = Mod(bottomOffset, itemHeight);
        if (rem < 0.5)
            rem = 0;

        var step = rem > 0 ? (itemHeight - rem) : itemHeight;

        var remaining = ExtentHeight - ViewportHeight - VerticalOffset;
        if (remaining <= 0)
            return 0;

        return Math.Min(step, Math.Min(remaining, ViewportHeight));
    }

    private static double Mod(double x, double m)
    {
        if (m <= 0)
            return 0;

        var r = x % m;
        return r < 0 ? r + m : r;
    }
}
