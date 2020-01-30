using System;
using System.Collections.Generic;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class CreateTagCommand : ICommand<Tag>
    {
        public Guid TypeId { get; }

        public string Name { get; }

        public bool HasValue { get; }

        public IReadOnlyCollection<string>? Synonyms { get; }

        public CreateTagCommand(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string>? synonyms)
        {
            TypeId = typeId;
            Name = name;
            HasValue = hasValue;
            Synonyms = synonyms;
        }
    }
}