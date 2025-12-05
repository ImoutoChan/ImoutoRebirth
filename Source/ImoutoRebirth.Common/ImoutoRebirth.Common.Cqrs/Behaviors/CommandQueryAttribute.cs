using System.Data;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class CommandQueryAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; }

    public bool NoTransaction { get; set; }

    public CommandQueryAttribute(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool noTransaction = false)
    {
        IsolationLevel = isolationLevel;
        NoTransaction = noTransaction;
    }
}
