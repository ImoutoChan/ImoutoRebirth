using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace ImoutoViewer.Converters
{
    internal class StringToInlineCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string + "<";
            str = str.Replace("&lt;", "<");
            str = str.Replace("&gt;", ">");
            str = str.Replace("\n", "");

            List<Inline> result = new List<Inline>();
            string currentText = String.Empty;
            string currentTag = String.Empty;
            Modifier flags = Modifier.None;

            int parseState = 0; // 1 - tag, 0 - text
            for (int i = 0; i < str.Length; i++)
            {
                if (!(new [] {'<', '>'}).Contains(str[i]))
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
                        currentTag = String.Empty;
                    }

                    parseState = 1;
                    if (String.IsNullOrEmpty(currentText))
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

                    currentText = String.Empty;
                    flags = Modifier.None;
                }
                else if (str[i] == '>')
                {
                    if (String.IsNullOrEmpty(currentTag))
                    {
                        continue;
                    }

                    switch (currentTag)
                    {
                        case "br":
                            result.Add(new LineBreak());
                            break;
                        case "i":
                            flags = flags | Modifier.Italic;
                            break;
                        case "b":
                            flags = flags | Modifier.Bold;
                            break;
                        case "tn":
                            flags = flags | Modifier.Small;
                            break;
                        case "big":
                            flags = flags | Modifier.Big;
                            break;
                        case "font size=\"3\"":
                            flags = flags | Modifier.ExtraBig;
                            break;
                        case "p class=\"tn\"":
                            flags = flags | Modifier.Small;
                            break;
                        default:
                            break;
                    }

                    parseState = 0;
                    currentTag = String.Empty;
                }
            }
            return new ObservableCollection<Inline>(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
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
}
