using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace ImoutoRebirth.Common.Cqrs.Behaviors
{
    internal static class CommandAttributeRetrieveLevelExtensions
    {
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;
        private static readonly ConcurrentDictionary<MemberInfo, IsolationLevel> Cache 
            = new ConcurrentDictionary<MemberInfo, IsolationLevel>();

        public static IsolationLevel GetIsolationLevel(this MemberInfo type) 
            => Cache.GetOrAdd(type, ExtractIsolationLevel);

        private static IsolationLevel ExtractIsolationLevel(MemberInfo type)
        {
            var commandAttribute = type.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;

            return commandAttribute?.IsolationLevel ?? DefaultIsolationLevel;
        }
    }
}