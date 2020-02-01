using System;
using System.Data;

namespace ImoutoRebirth.Common.Cqrs.Behaviors
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class CommandAttribute : Attribute
    {
        public IsolationLevel IsolationLevel { get; }

        public CommandAttribute(IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
        }
    }
}