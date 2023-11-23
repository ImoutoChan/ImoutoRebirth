using MassTransit.Testing;

namespace ImoutoRebirth.Common.MassTransit;

public static class TestsExtensions
{
    public static IEnumerable<T> SelectMessages<T>(this ISentMessageList sentMessages)
        where T : class
    {
        return sentMessages.Select<T>().Select(x => x.Context.Message);
    }
    
    public static bool AnyMessage<T>(this ISentMessageList sentMessages, Func<T,bool> predicate)
        where T : class
    {
        return sentMessages.Select<T>().Select(x => x.Context.Message).Any(predicate);
    }
}
