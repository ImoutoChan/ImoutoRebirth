using System.Globalization;

namespace ImoutoRebirth.Common;

public static class EnumExtensions
{
    /// <summary>
    /// Returns whether the given enum value is a defined value for its type.
    /// Throws if the type parameter is not an enum type.
    /// </summary>
    /// <remarks>
    /// Every enum types implement IConvertible interface,
    /// hence we use such constraints for generic type.
    /// </remarks>
    public static bool IsDefined<T>(this T enumValue) where T : struct, IConvertible
    {
        if (typeof(T).BaseType != typeof(Enum))
            throw new ArgumentException($"{nameof(T)} must be an enum type.");

        return EnumValueCache<T>.DefinedValues.Contains(enumValue.ToInt32(CultureInfo.InvariantCulture));
    }
    
    public static T ParseEnumOrDefault<T>(this string value, T defaultValue = default) where T : struct, IConvertible 
        => Enum.TryParse(value, true, out T result) ? result : defaultValue;

    /// <summary>
    /// Statically caches each defined value for each enum type for which this class is accessed.
    /// Uses the fact that static things exist separately for each distinct type parameter.
    /// </summary>
    private static class EnumValueCache<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static HashSet<int> DefinedValues { get; }

        static EnumValueCache()
        {
            if (typeof(T).BaseType != typeof(Enum))
                throw new Exception($"{nameof(T)} must be an enum type.");

            DefinedValues = new HashSet<int>((int[])Enum.GetValues(typeof(T)));
        }
    }
}
