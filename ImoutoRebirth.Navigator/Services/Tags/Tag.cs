using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal class Tag
    {
        public Guid? Id { get; }

        public string Title { get; }

        public TagType Type { get; }

        public IReadOnlyCollection<string> SynonymsCollection { get; }

        public bool HasValue { get; }
    }
}