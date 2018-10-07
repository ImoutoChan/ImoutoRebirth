using System;

namespace ImoutoRebirth.Room.Core.Models.Abstract
{
    public class ModelBase
    {
        public Guid Id { get; }

        public ModelBase(Guid id)
        {
            Id = id;
        }
    }
}