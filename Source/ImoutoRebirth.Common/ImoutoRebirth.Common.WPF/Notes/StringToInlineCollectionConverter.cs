using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace ImoutoRebirth.Common.WPF.Notes;

public class StringToInlineCollectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        var str = (value as string) + "<";
        str = str.Replace("&lt;", "<");
        str = str.Replace("&gt;", ">");
        str = str.Replace("\n", "<br>");

        var result = new List<Inline>();
        var currentText = string.Empty;
        var currentTag = string.Empty;
        var flags = Modifier.None;
        var tagBrackets = new[] { '<', '>' };

        var parseState = 0; // 1 - tag, 0 - text

        for (var i = 0; i < str.Length; i++)
        {
            if (!tagBrackets.Contains(str[i]))
            {
                if (parseState == 0)
                {
                    currentText += str[i];
                }
                else
                {
                    currentTag += str[i];
                }
            }
            else if (str[i] == '<')
            {
                if (parseState == 1)
                {
                    currentText = '<' + currentTag;
                    currentTag = string.Empty;
                }

                parseState = 1;
                if (string.IsNullOrEmpty(currentText))
                {
                    continue;
                }

                var run = new Run();
                if (flags.HasFlag(Modifier.Bold))
                {
                    run.FontWeight = FontWeights.Bold;
                }
                if (flags.HasFlag(Modifier.Italic))
                {
                    run.FontStyle = FontStyles.Italic;
                }
                if (flags.HasFlag(Modifier.Small))
                {
                    run.FontSize = 10;
                }
                if (flags.HasFlag(Modifier.Big))
                {
                    run.FontSize = 14;
                }
                if (flags.HasFlag(Modifier.ExtraBig))
                {
                    run.FontSize = 16;
                }
                run.Text = currentText;
                result.Add(run);

                currentText = string.Empty;
                flags = Modifier.None;
            }
            else if (str[i] == '>')
            {
                if (string.IsNullOrEmpty(currentTag))
                {
                    continue;
                }

                switch (currentTag)
                {
                    case "br":
                        result.Add(new LineBreak());
                        break;
                    case "i":
                        flags |= Modifier.Italic;
                        break;
                    case "b":
                        flags |= Modifier.Bold;
                        break;
                    case "tn":
                        flags |= Modifier.Small;
                        break;
                    case "big":
                        flags |= Modifier.Big;
                        break;
                    case "font size=\"3\"":
                        flags |= Modifier.ExtraBig;
                        break;
                    case "font size=\"+2\"":
                    case "font size=\"+1\"":
                        flags |= Modifier.Big;
                        break;
                    case "p class=\"tn\"":
                        flags |= Modifier.Small;
                        break;
                    default:
                        break;
                }

                parseState = 0;
                currentTag = string.Empty;
            }
        }
        return new ObservableCollection<Inline>(result);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    [Flags]
    private enum Modifier
    {
        None = 0,
        Bold = 2,
        Italic = 4,
        Small = 8,
        Big = 16,
        ExtraBig = 32
    }
}
