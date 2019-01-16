using System;

namespace ImoutoRebirth.Lilin.MessageContracts
{
    public interface IFileTag
    {
        string Type { get; }

        string Name { get; }

        string Value { get; }

        string[] Synonyms { get; }
    }
}