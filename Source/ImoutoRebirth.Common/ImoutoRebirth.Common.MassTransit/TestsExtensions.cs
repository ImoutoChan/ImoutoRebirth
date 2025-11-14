using MassTransit.Testing;

namespace ImoutoRebirth.Common.MassTransit;

public static class TestsExtensions
{
    extension(ISentMessageList sentMessages)
    {
        public IEnumerable<T> SelectMessages<T>() where T : class
            => sentMessages.Select<T>().Select(x => x.Context.Message);

        public bool AnyMessage<T>(Func<T,bool> predicate) where T : class
            => sentMessages.Select<T>().Select(x => x.Context.Message).Any(predicate);
    }
}
