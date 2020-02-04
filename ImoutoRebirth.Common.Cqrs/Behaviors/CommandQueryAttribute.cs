using System;
using System.Data;

namespace ImoutoRebirth.Common.Cqrs.Behaviors
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class CommandQueryAttribute : Attribute
    {
        public IsolationLevel IsolationLevel { get; }

        public CommandQueryAttribute(IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
        }
    }
}