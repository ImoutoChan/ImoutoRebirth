using System.Diagnostics.CodeAnalysis;

namespace ImoutoRebirth.Common.WebApi;

public class OptionalResponse<T>()
{
    public T? Value { get; set; }
    
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; set; }
    
    public static implicit operator T?(OptionalResponse<T> optional) => optional.Value;
    
    public static implicit operator OptionalResponse<T>(T? value) => new() { Value = value, HasValue = value != null };
}

public static class OptionalResponse
{
    public static async Task<OptionalResponse<T>> ToOptional<T>(this Task<T?> value)
    {
        var result = await value;
        return new() { Value = result, HasValue = result != null };
    }
}
