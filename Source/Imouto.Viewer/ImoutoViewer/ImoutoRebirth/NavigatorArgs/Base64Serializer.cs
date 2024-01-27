using System.Text;
using Newtonsoft.Json;

namespace ImoutoViewer.ImoutoRebirth.NavigatorArgs;

public static class Base64Serializer
{
    public static string Serialize<T>(T obj)
    {
        var serialized = JsonConvert.SerializeObject(obj);
        var serializedBytes = Encoding.UTF8.GetBytes(serialized.Compress());
        return Convert.ToBase64String(serializedBytes);
    }

    public static T Deserialize<T>(string str)
    {
        var bytes = Convert.FromBase64String(str);
        var compressed = Encoding.UTF8.GetString(bytes);
        var json = compressed.Decompress();
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}
