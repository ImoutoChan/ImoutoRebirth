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

        return -Math.Min(step, ViewportSize.Height);
    }

    protected override double GetLineDownScrollAmount()
    {
        var itemHeight = base.GetLineDownScrollAmount();

        var bottomOffset = VerticalOffset + ViewportSize.Height;
        var rem = Mod(bottomOffset, itemHeight);
        if (rem < 0.5)
            rem = 0;

        var step = rem > 0 ? (itemHeight - rem) : itemHeight;

        var remaining = ExtentHeight - ViewportSize.Height - VerticalOffset;
        if (remaining <= 0)
            return 0;

        return Math.Min(step, Math.Min(remaining, ViewportSize.Height));
    }

    private static double Mod(double x, double m)
    {
        if (m <= 0)
            return 0;

        var r = x % m;
        return r < 0 ? r + m : r;
    }
}
