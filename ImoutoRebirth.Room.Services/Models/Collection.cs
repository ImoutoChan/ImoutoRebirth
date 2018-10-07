using System;
using ImoutoRebirth.Room.Core.Models.Abstract;

namespace ImoutoRebirth.Room.Core.Models
{
    public class Collection : ModelBase
    {
        public string Name { get; }

        public Collection(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }
}
