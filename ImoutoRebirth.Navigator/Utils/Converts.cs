using System.Diagnostics;

namespace ImoutoRebirth.Navigator.Utils;

public static class Converts
{
    public static T To<T>(object value, bool enableException = false)
    {
        Type t = typeof(T);

        if (value is string && string.IsNullOrEmpty((string)value) && t != typeof(string))
            value = null;

        if (value == null || value == DBNull.Value)
            return default(T);
        else if (value is T)
            return (T)value;
        else
            try
            {
                if (!t.IsGenericType && t.IsEnum)
                    if (value is string)
                        return (T)Enum.ToObject(t, value);
                    else
                        return (T)value;

                if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    t = Nullable.GetUnderlyingType(t);
                return (T)Convert.ChangeType(value, t);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw new Exception("Ошибка конвертации в методе To", ex);
                Debug.WriteLine("Ошибка конвертации в методе To", "Converts");
                return default(T);
            }
    }
    public static bool TryTo<T>(object value, out T result)
    {
        try
        {
            result = To<T>(value, true);
        }
        catch
        {
            result = default(T);
            return false;
        }
        return true;
    }
    public static bool CanTo<T>(object value)
    {
        try
        {
            To<T>(value, true);
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static object To(object value, Type type)
    {
        Type t = typeof(Converts);
        var to = t.GetMethod("To", new Type[] { typeof(object) });
        return to.MakeGenericMethod(type).Invoke(null, new object[] { value });
    }

    public static object GetDefault(this Type type)
    {
        return typeof(Converts).GetMethod("GetDefault").MakeGenericMethod(type).Invoke(null, new Type[0]);
    }
    private static T GetDefault<T>()
    {
        return default(T);
    }
}