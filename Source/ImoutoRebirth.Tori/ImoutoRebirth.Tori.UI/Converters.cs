using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using ImoutoRebirth.Tori.UI.Models;

namespace ImoutoRebirth.Tori.UI;

public class StepToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InstallerStep current && parameter is InstallerStep step)
        {
            return current == step ? FontWeights.Bold : FontWeights.Normal;
        }
        
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StepToNextButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InstallerStep step)
        {
            return step switch
            {
                InstallerStep.Installation => "Finish",
                _ => "Next"
            };
        }
        
        return "Next";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LastStepConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InstallerStep step && parameter is ReadOnlyCollection<InstallerStep> steps)
        {
            // Don't show the ">" separator after the last step
            return step == steps[^1] ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}