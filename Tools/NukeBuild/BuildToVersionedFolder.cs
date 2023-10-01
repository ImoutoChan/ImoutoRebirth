using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<BuildToVersionedFolder>))]
public class BuildToVersionedFolder : Enumeration
{
    public static BuildToVersionedFolder True = new() { Value = nameof(True) };
    public static BuildToVersionedFolder False = new() { Value = nameof(False) };

    public static implicit operator string(BuildToVersionedFolder configuration)
    {
        return configuration.Value;
    }
}