using System;

namespace ImoutoRebirth.Lilin.MessageContracts
{
    public interface IFileTag
    {
        Guid TagId { get; }

        string Value { get; }
    }
}