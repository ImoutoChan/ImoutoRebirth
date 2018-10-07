using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ImoutoRebirth.Room.Database.Tools
{
    public class StringArrayValueConverter : ValueConverter<string[], string>
    {
        private const char Separator = ';';
        private static readonly string SeparatorString = Separator.ToString();

        public StringArrayValueConverter(ConverterMappingHints mappingHints = null) 
            : base(
                strings => string.Join(SeparatorString, strings), 
                s => s.Split(Separator).ToArray(), 
                mappingHints)
        {
        }
    }
}