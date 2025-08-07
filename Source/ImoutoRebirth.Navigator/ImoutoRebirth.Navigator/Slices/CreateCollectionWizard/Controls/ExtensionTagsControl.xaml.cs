using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard.Controls;

public partial class ExtensionTagsControl : UserControl
{
    public static readonly DependencyProperty ExtensionsProperty =
        DependencyProperty.Register(
            nameof(Extensions),
            typeof(ObservableCollection<string>),
            typeof(ExtensionTagsControl),
            new PropertyMetadata(null));

    public ObservableCollection<string>? Extensions
    {
        get => (ObservableCollection<string>?)GetValue(ExtensionsProperty);
        set => SetValue(ExtensionsProperty, value);
    }

    public ExtensionTagsControl() => InitializeComponent();

    private void OnAddButtonClick(object sender, RoutedEventArgs e) => AddExtension();

    private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: string extension }) 
            Extensions?.Remove(extension);
    }

    private void OnNewExtensionKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddExtension();
            e.Handled = true;
            return;
        }

        var formattedText = new FormattedText(
            ExpandingTextBox.Text + e.Key.ToString()[..1],
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new(ExpandingTextBox.FontFamily, ExpandingTextBox.FontStyle, ExpandingTextBox.FontWeight, ExpandingTextBox.FontStretch),
            ExpandingTextBox.FontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        var newWidth = formattedText.Width + 28;
        OvalBorder.Width = Math.Max(newWidth, 50);
    }

    private void AddExtension()
    {
        if (Extensions == null || ExpandingTextBox == null)
            return;

        var extensionText = ExpandingTextBox.Text.Trim().ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(extensionText))
            return;

        extensionText = extensionText.Trim('.');

        if (!string.IsNullOrEmpty(extensionText) && !Extensions.Contains(extensionText))
        {
            Extensions.Add(extensionText);
            ExpandingTextBox.Text = "";
        }
    }
}