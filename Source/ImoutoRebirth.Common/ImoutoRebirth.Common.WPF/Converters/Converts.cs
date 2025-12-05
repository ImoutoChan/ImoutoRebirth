namespace ImoutoRebirth.Common.WPF.Converters;

public static class Converts
{
    public static ToT? To<ToT>(object? value, bool enableException = false)
    {
        Type t = typeof(ToT);

        if (value is string stringValue
            && string.IsNullOrEmpty(stringValue)
            && t != typeof(string))
        {
            value = null;
        }

        if (value == null || value == DBNull.Value)
        {
            return default;
        }
        else if (value is ToT toT)
        {
            return toT;
        }
        else
        {
            try
            {
                if (t is
                    {
                        IsGenericType: false,
                        IsEnum: true
                    })
                {
                    if (value is string)
                        return (ToT)Enum.ToObject(t, value);

                    return (ToT)value;
                }

                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    t = Nullable.GetUnderlyingType(t)!;
                }
                return (ToT)Convert.ChangeType(value, t);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw new InvalidOperationException("Unable to convert object to " + t.Name, ex);

                return default;
            }
        }
    }
}
